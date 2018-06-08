using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;

public enum NetworkMode
{
    Single,
    Lan,
    Net
}


public class NetworkMangerCustom :  NetworkManager{//自定义一个NetworkManger类 处理networkmanger事务

    public static NetworkMode MyMode;

    public static void SingleGame()
    {
        MyMode = NetworkMode.Single;
        singleton.StartHost(singleton.connectionConfig, 1);//单例模式同时创建一个服务器和客户端，当前的连接配置，maxContention最大连接数量
    }
    public static void LanGame()
    {

        //singleton 当前networkManger的单例模式
        MyMode = NetworkMode.Lan;
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



    public static void NetGame()
    {
        MyMode = NetworkMode.Net;
        singleton.StartMatchMaker();//牵线搭桥功能，启用Unet网络对战功能
        singleton.matchMaker.ListMatches(0, 20, "", false, 0, 0, singleton.OnMatchList);
                    //第一个参数：startpagenumber：表示第几页的list
                    //第二个参数：表示的是每一页有多少个
                    //第三个参数：表示需要找到的房间名称
                    //第四个参数：表示是否返回带有密钥的房间
                    //第五个参数：表示的是竞赛方面的设置，暂时可以不管，设为0，
                    //第六个参数：表示的是一个域，只从这个域值放回房间
                    //第七个参数：是一个回调的函数
    }

    public override void OnMatchList(bool success, string extendedInfo, List<UnityEngine.Networking.Match.MatchInfoSnapshot> matchList)
    {
        if (!success)
        {
            return;//调用不成功
        }
        if (matchList != null)//安全校验 列表不为空
        {
            List<MatchInfoSnapshot> availableMatches = new List<MatchInfoSnapshot>();
            foreach (MatchInfoSnapshot match in matchList)
            {
                if (match.currentSize < match.maxSize)
                {
                    availableMatches.Add(match);//当前房间玩家数<限制的最大玩家数（房间未满）就提取出来
                }
            }

            //如果列表数为零，为零只能创建服务器，否则就加入服务器
            if (availableMatches.Count == 0)
            {
                //创建服务器
                CreateMatch();
            }
            else
            {
                //加入服务器
                matchMaker.JoinMatch(availableMatches[Random.Range(0, availableMatches.Count - 1)].networkId, "", "", "", 0, 0, OnMatchJoined);
                    //第一个NetworkID
                    //第二个口令 空
                    //第三个公网IP地址 空
                    //第四个私网地址 空
                    //第五个层？=0
                    //第六个域=0
            }
        }
    }

    void CreateMatch()//告诉Unet创建服务器
    {
        matchMaker.CreateMatch("", matchSize, true, "", "", "", 0, 0, OnMatchCreate);
                    //第一个参数：房间名称
                    //第二个参数：房间可玩的玩家数量
                    //第三个参数：暂时设为true
                    //第四个参数：口令
                    //第五个参数：ClientIP地址（公网）
                    //第六个参数：私网地址
                    //第七个参数：调用回调函数
    }

    public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        if (!success)
        {
            return;
        }
        StartHost(matchInfo);//利用Unet返回的matchInfo创建服务器
    }
    public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        if (!success)
        {
            int currentScene = SceneManager.GetActiveScene().buildIndex; //当前场景加载，防止回调没成功导致出错死机
            SceneManager.LoadScene(currentScene);//重新加载
            return;
        }
        StartClient(matchInfo);//利用Unet传回的matchInfo启动客户端
    }
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        //print(234345);
        int teamIndex = GameManager.GetInstance().GetTeamFill();//获取应该分配的组号
        Vector3 startPos = GameManager.GetInstance().GetSpawnPosition(teamIndex);//获取出生点
        GameObject player = Instantiate(playerPrefab, startPos, Quaternion.identity);//初始化player预制体游戏对象(使用默认的预制体在服务器创建玩家）

        //为Player的组号赋值
        Player p = player.transform.GetComponent<Player>();
        p.teamIndex = teamIndex;

        GameManager.GetInstance().size[teamIndex]++;//该组人数++
        GameManager.GetInstance().ui.OnTeamSizeChanged(SyncListInt.Operation.OP_DIRTY, teamIndex);

        //告诉server是哪个玩家id连接的：连接，预制体，连接id
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }
	
}
