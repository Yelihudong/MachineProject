using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class NetworkDiscoveryCustom : NetworkDiscovery {

    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        //取到广播包的话
        StopBroadcast();//NetworkMangerCustom里面的running()就会暂停

        NetworkManager.singleton.networkAddress = fromAddress;//修改地址
        NetworkManager.singleton.StartClient();
    }
}
