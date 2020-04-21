using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SilderOutputScript : MonoBehaviour {

    private Text text;

    private void Start() {
        text = GetComponent<Text>();
    }

    public void ChangeValue(Slider slider) {
        text.text = slider.value.ToString();
    }

}
