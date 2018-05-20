using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkMangerCustom :  NetworkManager{

    public static void LanGame()
    {
        //singleton 当前networkManger的单例模式
        singleton.StartCoroutine((singleton as NetworkMangerCustom).DiscoveryNetwork());//找不到 转换后在找
    }

    //协程
    public IEnumerator DiscoveryNetwork()
    {
        //取得Discovery组件
        NetworkDiscoveryCustom discovery = GetComponent<NetworkDiscoveryCustom>();
        discovery.Initialize();//组件初始化
        discovery.StartAsClient();//扫描局域网服务器，有的话直接加入。到这里会直接调用Discovery里重写的方法，
        yield return new WaitForSeconds(2);

        //没有找到服务器，就建立
        if (discovery.running)//还在跑 没有加入
        {
            discovery.StopBroadcast();//停掉广播包
            yield return new WaitForSeconds(.5f);

            discovery.StartAsServer();//作为服务器发射广播包
            StartHost();//同时作为服务器和客户端启动
            //StartClient();//作为客户端启动
            //StartServer();//只作为服务器启动
        }

    }
	
}
