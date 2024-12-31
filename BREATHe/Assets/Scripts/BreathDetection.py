import pyaudio
import numpy as np
import matplotlib
matplotlib.use('TkAgg')  # 设置后端为TkAgg
import matplotlib.pyplot as plt
from matplotlib.animation import FuncAnimation
import time
import socket
import json
import threading
import tkinter as tk
from tkinter import ttk
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg

# 修改字体配置
plt.rcParams['font.sans-serif'] = ['Microsoft YaHei', 'SimHei', 'DejaVu Sans', 'Bitstream Vera Sans', 'sans-serif']  # 添加多个备选字体
plt.rcParams['axes.unicode_minus'] = False  # 解决负号显示问题
plt.rcParams['font.family'] = 'sans-serif'  # 设置字体族

# 音频参数
CHUNK = 2048
FORMAT = pyaudio.paFloat32
CHANNELS = 1
RATE = 44100

# 呼吸检测参数
breath_count = 0  # 呼吸次数
THRESHOLD = 0.002     # 初始阈值
MAX_THRESHOLD = 0.05     # 呼吸的最高强度阈值
LOW_THRESHOLD_FACTOR = 0.3  # 呼吸结束的低强度阈值占比
is_above_threshold = False  # 标记是否处于呼吸周期

# 最小呼吸持续时间
MIN_BREATH_DURATION = 0.15  # 最小呼吸周期为 0.15 秒

# UDP 配置
HOST = '127.0.0.1'  # localhost
PORT = 65432
udp_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
udp_socket.setblocking(0)  # 设置为非阻塞模式

# 初始化 PyAudio
p = pyaudio.PyAudio()
stream = p.open(
    format=FORMAT,
    channels=CHANNELS,
    rate=RATE,
    input=True,
    frames_per_buffer=CHUNK
)

# 用于存储呼吸事件的时间戳
breath_events = []

# 创建控制面板窗口
control_window = tk.Tk()
control_window.title("呼吸检测控制面板")
control_window.geometry("800x700")  # 调整窗口大小

# 创建主滚动框架
main_canvas = tk.Canvas(control_window)
scrollbar = ttk.Scrollbar(control_window, orient="vertical", command=main_canvas.yview)
scrollable_frame = ttk.Frame(main_canvas)

scrollable_frame.bind(
    "<Configure>",
    lambda e: main_canvas.configure(scrollregion=main_canvas.bbox("all"))
)

main_canvas.create_window((0, 0), window=scrollable_frame, anchor="nw")
main_canvas.configure(yscrollcommand=scrollbar.set)

# 添加鼠标滚轮支持
def _on_mousewheel(event):
    main_canvas.yview_scroll(int(-1*(event.delta/120)), "units")

# 直接绑定到控制窗口
control_window.bind_all("<MouseWheel>", _on_mousewheel)

# 创建呼吸状态指示器（移到最上面）
status_frame = ttk.LabelFrame(scrollable_frame, text="呼吸状态", padding="10")
status_frame.pack(fill="x", padx=10, pady=5)

# 创建主状态显示框架
main_status_frame = ttk.Frame(status_frame)
main_status_frame.pack(expand=True, fill="x")

# 创建左侧空白框架（用于居中）
left_spacer = ttk.Frame(main_status_frame)
left_spacer.pack(side="left", expand=True, fill="x")

# 创建中央状态显示框架
center_status_frame = ttk.Frame(main_status_frame)
center_status_frame.pack(side="left")

# 创建呼吸指示器框架
indicator_frame = ttk.Frame(center_status_frame)
indicator_frame.pack(pady=5)

# 创建更大的呼吸状态指示器
breath_status_indicator = tk.Canvas(indicator_frame, width=40, height=40, bg=control_window.cget('bg'))
breath_status_indicator.pack(pady=5)
# 在画布中央创建圆形
breath_status_indicator.create_oval(5, 5, 35, 35, fill='gray', tags='status', width=2, outline='#666666')

# 创建状态文本框架
status_text_frame = ttk.Frame(center_status_frame)
status_text_frame.pack()

breath_count_label = ttk.Label(status_text_frame, text="呼吸次数: 0", font=('Arial', 12))
breath_count_label.pack(pady=2)

frequency_label = ttk.Label(status_text_frame, text="频率: 0 次/10秒", font=('Arial', 12))
frequency_label.pack(pady=2)

# 创建右侧空白框架（用于居中）
right_spacer = ttk.Frame(main_status_frame)
right_spacer.pack(side="left", expand=True, fill="x")

# 创建matplotlib图形框架（调整大小）
plot_frame = ttk.Frame(scrollable_frame)
plot_frame.pack(fill="both", expand=True, padx=10, pady=5)

# 初始化绘图（减小图形大小）
fig, (ax1, ax2) = plt.subplots(2, 1, figsize=(8, 4))
canvas = FigureCanvasTkAgg(fig, master=plot_frame)
canvas.draw()
canvas.get_tk_widget().pack(fill="both", expand=True)

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

# 修改阈值调整框架，添加数值输入
def create_threshold_control(parent, label, var_name, from_, to_, default):
    frame = ttk.Frame(parent)
    frame.pack(fill="x", pady=5)
    
    label = ttk.Label(frame, text=label, width=15)
    label.pack(side="left", padx=5)
    
    value_var = tk.StringVar(value=str(default))
    
    # 将线性值转换为对数刻度值
    def to_log_scale(value):
        return np.log10(value)
    
    # 将对数刻度值转换回线性值
    def from_log_scale(log_value):
        return 10 ** log_value
    
    # 将滑块位置转换为实际值
    def slider_to_value(slider_pos):
        # 将0-100的滑块位置转换为对数范围内的值
        log_range = log_to - log_from
        log_value = log_from + (log_range * float(slider_pos) / 100)
        return from_log_scale(log_value)
    
    # 将实际值转换为滑块位置
    def value_to_slider(value):
        # 将实际值转换为0-100的滑块位置
        log_value = to_log_scale(value)
        log_range = log_to - log_from
        return ((log_value - log_from) / log_range) * 100
    
    def on_slider_change(event):
        try:
            slider_pos = float(slider.get())
            actual_value = slider_to_value(slider_pos)
            value_var.set(f"{actual_value:.6f}")
            update_threshold(var_name, actual_value)
        except:
            pass
    
    def on_entry_change(*args):
        try:
            value = float(value_var.get())
            if from_ <= value <= to_:
                slider_pos = value_to_slider(value)
                slider.set(slider_pos)
                update_threshold(var_name, value)
        except:
            pass
    
    # 使用对数刻度的范围
    log_from = to_log_scale(from_)
    log_to = to_log_scale(to_)
    
    # 创建0-100范围的滑块
    slider = ttk.Scale(frame, from_=0, to=100, orient="horizontal", 
                      length=200, command=lambda x: on_slider_change(x))
    # 设置初始滑块位置
    initial_slider_pos = value_to_slider(default)
    slider.set(initial_slider_pos)
    slider.pack(side="left", padx=5)
    
    entry = ttk.Entry(frame, textvariable=value_var, width=10)
    entry.pack(side="left", padx=5)
    
    value_var.trace_add("write", on_entry_change)
    
    return slider, value_var

def update_threshold(var_name, value):
    global THRESHOLD, MAX_THRESHOLD, LOW_THRESHOLD_FACTOR
    if var_name == "min":
        THRESHOLD = value
    elif var_name == "max":
        MAX_THRESHOLD = value
    elif var_name == "factor":
        LOW_THRESHOLD_FACTOR = value

# 创建阈值调整框架
threshold_frame = ttk.LabelFrame(scrollable_frame, text="阈值调整", padding="10")
threshold_frame.pack(fill="x", padx=10, pady=5)

min_slider, min_var = create_threshold_control(threshold_frame, "最小阈值:", "min", 0.0001, 0.1, THRESHOLD)
max_slider, max_var = create_threshold_control(threshold_frame, "最大阈值:", "max", 0.001, 0.5, MAX_THRESHOLD)
factor_slider, factor_var = create_threshold_control(threshold_frame, "低阈值因子:", "factor", 0.1, 0.9, LOW_THRESHOLD_FACTOR)

# 在文件开头的全局变量部分添加
update_task = None  # 用于存储更新任务的ID

# 替换原来的 update_status_display 函数
def update_status_display():
    global update_task
    
    if not control_window.winfo_exists():
        return
        
    try:
        breath_count_label.config(text=f"呼吸次数: {breath_count}")
        if 'frequency' in globals():
            frequency_label.config(text=f"频率: {frequency} 次/10秒")
        
        if is_above_threshold:
            breath_status_indicator.itemconfig('status', fill='#FF4444', outline='#CC0000')
        else:
            breath_status_indicator.itemconfig('status', fill='#CCCCCC', outline='#666666')
            
        update_task = control_window.after(100, update_status_display)
    except Exception as e:
        print(f"更新状态时出错: {e}")

# 修改 cleanup 函数
def cleanup():
    global update_task, running
    running = False
    
    if update_task is not None:
        try:
            control_window.after_cancel(update_task)
            update_task = None
        except:
            pass
    
    try:
        stream.stop_stream()
        stream.close()
        p.terminate()
        udp_socket.close()
        plt.close()
        control_window.quit()
    except:
        pass

# 修改 main_loop 函数
def main_loop():
    global running
    try:
        running = True
        update_status_display()  # 只在这里启动一次更新循环
        control_window.mainloop()
    except Exception as e:
        print(f"主循环错误: {e}")
    finally:
        cleanup()

# 确保在窗口关闭时调用 cleanup
control_window.protocol("WM_DELETE_WINDOW", cleanup)

# 启动时间
start_time = time.time()
last_time = start_time
last_intensity = 0
breath_start_intensity = 0
breath_start_time = None  # 初始化呼吸开始时间
print("开始检测呼吸...")

# 在全局变量部分添加
MAX_HISTORY_POINTS = 200  # 减少历史数据点数
DISPLAY_TIME_WINDOW = 5   # 显示最近5秒的数据
UPDATE_INTERVAL = 50      # 更新间隔（毫秒）

# 在呼吸检测参数部分添加
CALIBRATION_TIME = 3  # 校准时（秒）
MOVING_AVERAGE_WINDOW = 3  # 减小移动平均窗口大小
SMOOTHING_FACTOR = 0.3  # 指数移动平均的平滑因子
noise_baseline = 0  # 噪音基线值
is_calibrating = True  # 校准标志
calibration_start_time = 0  # 校准开始时间
calibration_samples = []  # 校准样本

# 控制面板部分添加校准按钮
def start_calibration():
    global is_calibrating, calibration_start_time, calibration_samples, noise_baseline
    is_calibrating = True
    calibration_start_time = time.time()
    calibration_samples = []
    calibration_button.config(state='disabled')
    calibration_label.config(text="正在校准...")
    print("开始校准底噪...")

# 创建校准框架
calibration_frame = ttk.LabelFrame(scrollable_frame, text="底噪校准", padding="10")
calibration_frame.pack(fill="x", padx=10, pady=5)

calibration_button = ttk.Button(calibration_frame, text="开始校准", command=start_calibration)
calibration_button.pack(side="left", padx=5)

calibration_label = ttk.Label(calibration_frame, text="未校准")
calibration_label.pack(side="left", padx=5)

noise_baseline_label = ttk.Label(calibration_frame, text="底噪基线: 0.000")
noise_baseline_label.pack(side="left", padx=5)

# 更新函数
def update(frame):
    global is_above_threshold, last_time, last_intensity, breath_start_intensity, breath_start_time
    global breath_end_time, breath_count, frequency, times, intensities
    global breath_events, is_calibrating, noise_baseline, calibration_samples

    try:
        # 读取音频数据
        data = stream.read(CHUNK, exception_on_overflow=False)
        audio_data = np.frombuffer(data, dtype=np.float32)
        raw_intensity = float(np.abs(audio_data).mean())
    except Exception as e:
        print(f"读取音频数据出错: {e}")
        return line1, line2

    # 校准逻辑
    if is_calibrating:
        calibration_samples.append(raw_intensity)
        current_calibration_time = time.time() - calibration_start_time
        
        if current_calibration_time >= CALIBRATION_TIME:
            noise_baseline = np.percentile(calibration_samples, 50)
            is_calibrating = False
            calibration_button.config(state='normal')
            calibration_label.config(text="校准完成")
            noise_baseline_label.config(text=f"底噪基线: {noise_baseline:.4f}")
            print(f"校准完成，底噪基线: {noise_baseline:.4f}")
            return line1, line2

    # 减去底噪
    intensity = max(0, raw_intensity - noise_baseline)
    current_time = time.time() - start_time

    # 数据存储优化：只保留最近的数据点
    times.append(current_time)
    intensities.append(intensity)
    
    # 仅保留显示窗口内的数据
    cutoff_time = current_time - DISPLAY_TIME_WINDOW
    while times and times[0] < cutoff_time:
        times.pop(0)
        intensities.pop(0)

    # 呼吸检测逻辑
    breath_state_changed = False
    if THRESHOLD < intensity < MAX_THRESHOLD:
        if not is_above_threshold:
            is_above_threshold = True
            breath_start_time = current_time
            breath_start_intensity = intensity
            last_time = current_time
            breath_state_changed = True
            print(f"🔴 呼吸开始！时间: {current_time:.2f} 秒, 强度: {intensity:.4f}")
        else:
            if intensity > breath_start_intensity:
                breath_start_intensity = intensity
                print(f"实时强度: {intensity:.4f}")
    else:
        if is_above_threshold:
            if intensity < breath_start_intensity * LOW_THRESHOLD_FACTOR:
                is_above_threshold = False
                breath_end_time = current_time
                breath_duration = breath_end_time - breath_start_time
                breath_state_changed = True

                if breath_duration >= MIN_BREATH_DURATION:
                    breath_count += 1
                    # 优化呼吸事件存储：只保留最近10秒的事件
                    breath_events = [t for t in breath_events if current_time - t <= 10]
                    breath_events.append(current_time)
                    frequency = len(breath_events)
                    print(f"🔵 呼吸结束 -> 频率: {frequency} 次/10秒, 持续: {breath_duration:.2f}秒")
                else:
                    print(f"⚠️ 呼吸周期过短，忽略：{breath_duration:.2f} 秒")

                last_time = breath_end_time

    # 优化数据发送：使用非阻塞方式发送数据
    try:
        if breath_state_changed:
            data = {
                'type': 'state_change',
                'is_breathing': is_above_threshold,
                'time': current_time,
                'intensity': intensity,
                'frequency': frequency if 'frequency' in locals() else 0,
                'breath_count': breath_count
            }
        else:
            data = {
                'type': 'update',
                'is_breathing': is_above_threshold,
                'intensity': intensity
            }
        
        udp_socket.sendto(json.dumps(data).encode(), (HOST, PORT))
    except BlockingIOError:
        pass
    except Exception as e:
        print(f"发送数据时出错: {e}")

    # 优化图形更新
    if frame % 2 == 0:  # 每两帧更新一次波形图
        line1.set_ydata(audio_data)
        ax1.set_ylim(audio_data.min() * 1.1, audio_data.max() * 1.1)

    # 更新强度历史图
    line2.set_data(times, intensities)
    
    # 优化坐标轴更新
    if frame % 5 == 0:  # 降低坐标轴更新频率
        ax2.set_xlim(max(0, current_time - DISPLAY_TIME_WINDOW), current_time + 0.1)
        if intensities:
            ax2.set_ylim(0, max(0.1, max(intensities) * 1.2))

    return line1, line2

# 修改动画更新
ani = FuncAnimation(fig, update, interval=UPDATE_INTERVAL, cache_frame_data=False)

# 在main_loop函数之前，添加滚动条和画布的布局
main_canvas.pack(side="left", fill="both", expand=True)
scrollbar.pack(side="right", fill="y")

if __name__ == "__main__":
    main_loop()
