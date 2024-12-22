import pyaudio
import numpy as np
import matplotlib.pyplot as plt
from matplotlib.animation import FuncAnimation
import time
import socket
import json
import threading

# 添加以下配置
#plt.rcParams['font.sans-serif'] = ['Arial Unicode MS']  # Mac系统
# 或者使用
plt.rcParams['font.sans-serif'] = ['SimHei']  # Windows系统
plt.rcParams['axes.unicode_minus'] = False  # 解决负号显示问题

# 音频参数
CHUNK = 2048
FORMAT = pyaudio.paFloat32
CHANNELS = 1
RATE = 44100

# 呼吸检测参数
THRESHOLD = 0.005        # 初始阈值
MAX_THRESHOLD = 0.2     # 呼吸的最高强度阈值
LOW_THRESHOLD_FACTOR = 0.3  # 呼吸结束的低强度阈值占比
is_above_threshold = False  # 标记是否处于呼吸周期

# 时间流速因子参数
CALIBRATION_FACTOR = 100  # 调整时间流速映射比例
MIN_TIME_SCALE = 0.1      # 时间流速因子的最小值
MAX_TIME_SCALE = 5.0      # 时间流速因子的最大值

# 滑动窗口参数
WINDOW_SIZE = 5
smoothed_factors = []  # 滑动窗口存储时间流速因子

# 最小呼吸持续时间
MIN_BREATH_DURATION = 0.15  # 最小呼吸周期为 0.5 秒

# UDP 配置
HOST = '127.0.0.1'  # localhost
PORT = 65432
udp_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# 初始化 PyAudio
p = pyaudio.PyAudio()
stream = p.open(
    format=FORMAT,
    channels=CHANNELS,
    rate=RATE,
    input=True,
    frames_per_buffer=CHUNK
)

# 接收阈值更新信息
def receive_threshold():
    global THRESHOLD
    print("[Threshold Thread] 开始监听阈值更新...")
    while True:
        try:
            data, addr = udp_socket.recvfrom(1024)
            message = data.decode('utf-8')
            print(f"[Threshold Thread] 收到原始数据: {message}")
            
            threshold_data = json.loads(message)
            print(f"[Threshold Thread] 解析后的数据: {threshold_data}")
            
            if "min_threshold" in threshold_data and "max_threshold" in threshold_data:
                min_threshold = threshold_data["min_threshold"]
                max_threshold = threshold_data["max_threshold"]
                print(f"[Threshold Thread] 更新阈值 - 最小值: {min_threshold:.6f}, 最大值: {max_threshold:.6f}")
                THRESHOLD = min_threshold
                MAX_THRESHOLD = max_threshold
            elif "threshold" in threshold_data:
                THRESHOLD = threshold_data.get("threshold", 0.001)
                print(f"[Threshold Thread] 更新单一阈值: {THRESHOLD:.6f}")
            else:
                print(f"[Threshold Thread] 警告：收到未知格式的阈值数据")
                
        #except json.JSONDecodeError as e:
            #print(f"[Threshold Thread] JSON解析错误: {e}")
        except Exception as e:
            print(f"[Threshold Thread] 接收阈值时发生错误: {e}")
            time.sleep(1)  # 发生错误时等待一秒再继续

# 初始化绘图
fig, (ax1, ax2) = plt.subplots(2, 1, figsize=(10, 6))
x = np.arange(0, CHUNK)
line1, = ax1.plot(x, np.zeros(CHUNK))
times = []
intensities = []
line2, = ax2.plot([], [])

# 配置波形图
ax1.set_title("实时音频波形")
ax1.set_ylim(-0.5, 0.5)
ax1.set_xlim(0, CHUNK)
ax1.set_xlabel("采样点")
ax1.set_ylabel("强度")

# 配置强度历史图
ax2.set_title("强度历史记录")
ax2.set_xlabel("时间 (秒)")
ax2.set_ylabel("强度")
ax2.grid(True)

# 启动时间
start_time = time.time()
last_time = start_time
last_intensity = 0
breath_start_intensity = 0
breath_start_time = None  # 初始化呼吸开始时间
print("开始检测呼吸...")

# 更新函数
def update(frame):
    global is_above_threshold, last_time, last_intensity, breath_start_intensity, smoothed_factors, breath_start_time

    # 读取音频数据
    data = stream.read(CHUNK, exception_on_overflow=False)
    audio_data = np.frombuffer(data, dtype=np.float32)
    intensity = float(np.abs(audio_data).mean())  # 确保转换为标准 Python float 类型
    current_time = time.time() - start_time

    # 动态调整波形范围
    line1.set_ydata(audio_data)
    ax1.set_ylim(audio_data.min() * 1.1, audio_data.max() * 1.1)

    # 呼吸检测逻辑
    if THRESHOLD < intensity < MAX_THRESHOLD:  # 添加最大阈值过滤
        if not is_above_threshold:
            # 检测到呼吸开始
            is_above_threshold = True
            breath_start_time = current_time  # 记录呼吸开始时间
            breath_start_intensity = intensity
            last_time = current_time  # 重置起始时间为当前时间
            print(f"🔴 呼吸开始！时间: {current_time:.2f} 秒, 强度: {intensity:.4f}")
        else:
            # 持续计算时间流速因子
            delta_intensity = intensity - last_intensity
            delta_time = current_time - last_time
            if delta_time > 0:
                time_scale_factor = abs(delta_intensity) / delta_time * CALIBRATION_FACTOR
                smoothed_factors.append(time_scale_factor)
                if len(smoothed_factors) > WINDOW_SIZE:
                    smoothed_factors.pop(0)

                # 平滑时间流速因子
                smoothed_time_scale = sum(smoothed_factors) / len(smoothed_factors)
                smoothed_time_scale = max(MIN_TIME_SCALE, min(smoothed_time_scale, MAX_TIME_SCALE))

                print(f"实时强度: {intensity:.4f}, 平滑时间流速因子: {smoothed_time_scale:.2f}")

                # 发送数据到 Unity
                data_to_send = {
                    'time': current_time,
                    'intensity': intensity,
                    'time_scale': smoothed_time_scale
                } 
                try:
                    udp_socket.sendto(json.dumps(data_to_send).encode(), (HOST, PORT))
                    #print(f"发送数据包到 Unity: {data_to_send}")
                except Exception as e:
                    #print(f"发送数据时出错: {e}")
                    pass

            last_time = current_time
    else:
        if is_above_threshold and intensity < breath_start_intensity * LOW_THRESHOLD_FACTOR:
            # 检测到呼吸结束
            is_above_threshold = False
            breath_end_time = current_time
            breath_duration = breath_end_time - breath_start_time  # 基于呼吸开始时间计算

            if breath_duration < MIN_BREATH_DURATION:
                print(f"⚠️ 呼吸周期过短，忽略：{breath_duration:.2f} 秒")
            else:
                print(f"🔵 呼吸结束！持续时间: {breath_duration:.2f} 秒")

                # 发送结束事件到 Unity
                data_to_send = {
                    'time': current_time,
                    'breath_duration': breath_duration
                }
                udp_socket.sendto(json.dumps(data_to_send).encode(), (HOST, PORT))

            # 更新时间
            last_time = breath_end_time

    # 更新强度历史记录
    times.append(current_time)
    intensities.append(intensity)
    line2.set_data(times, intensities)
    ax2.set_xlim(max(0, current_time - 10), current_time + 0.5)  # 显示最近10秒
    ax2.set_ylim(0, max(0.1, max(intensities) * 1.2))

    last_intensity = intensity  # 更新最后的强度

    return line1, line2

# 动画
ani = FuncAnimation(fig, update, interval=10, cache_frame_data=False)

# 接收阈值更新的线程
threshold_thread = threading.Thread(target=receive_threshold)
threshold_thread.daemon = True
threshold_thread.start()

plt.show()

# 退出时清理
stream.stop_stream()
stream.close()
p.terminate()
udp_socket.close()
