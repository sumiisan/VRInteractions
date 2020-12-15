using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour {
    public static HUD shared;
    public Text text;

    private Dictionary<string, string> props = new Dictionary<string, string> ();    
    // Start is called before the first frame update
    void Start() {
       shared = this;
    } 

    public void SetProp(string name, string value) {
        props[name] = value;
        UpdateText();
    }

    void UpdateText() {
        string s = "";

        foreach( KeyValuePair<string, string> kv in props ) {
            s += kv.Key + "=" +kv.Value + "\n";
        }
        text.text = s;
    }

    // Update is called once per frame
    void Update() {
        
    }
}
