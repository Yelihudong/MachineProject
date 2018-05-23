using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndexUI : MonoBehaviour {

    public void SingleButtonClick()
    {
        NetworkMangerCustom.SingleGame();
    }
    public void NetButtonClick()
    {
        NetworkMangerCustom.NetGame();
    }
    public void LanButtonClick()
    {
        NetworkMangerCustom.LanGame();
    }
    
}
