using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//使用方法：
//1. 继承MonosingletonTemp<T>类，其中T是你的类名
//2. 区别Instance和_instance的区别：Instance是一个属性，用于获取单例对象，_instance是一个字段，用于存储单例对象
//3. 如果我希望调用某个脚本（比如SoundManager）的单例对象，只需要SoundManager.Instance即可
//4. 调用某个脚本后，我可以通过SoundManager.Instance.PlayMusic()这样的方式调用SoundManager脚本中的方法
public class MonosingletonTemp<T> : MonoBehaviour where T : MonosingletonTemp<T>
{
    
    [SerializeField]public bool isDonDestroyOnLoad = true;
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject singletonObject = new GameObject(typeof(T).Name); //创建一个新的GameObject
                _instance = singletonObject.AddComponent<T>();//将脚本添加到GameObject上
                if (_instance.isDonDestroyOnLoad)
                {
                    DontDestroyOnLoad(singletonObject);
                }
                
                // DontDestroyOnLoad(singletonObject);
            }
            return _instance;
        }
    }


    void Awake()
    {

        if (_instance == null)
        {
            _instance = this as T;
            if (isDonDestroyOnLoad)
            {
                DontDestroyOnLoad(this.gameObject);
            }
            
            // DontDestroyOnLoad(this.gameObject);
        }
        else if (_instance != this)
        {
            Destroy(this.gameObject);
        }

    }
}