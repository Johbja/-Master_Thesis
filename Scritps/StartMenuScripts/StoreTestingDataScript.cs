using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StoreTestingDataScript : MonoBehaviour {

    [SerializeField] private InputField[] textFields;
    [SerializeField] private Slider slider;
    [SerializeField] private Text errorText;
    [SerializeField] private int TestOder;
    [SerializeField] private string buildVersion;

    public void StoreData() {

        string[] data = new string[textFields.Length + 2];

        data[0] = "Data from test: " + TestOder; 

        for(int i = 0; i < textFields.Length; i++) {
            if(textFields[i].text.Length < 2) {
                errorText.gameObject.SetActive(true);
                return;
            } else {
                data[i + 1] = textFields[i].text;
            }
        }

        data[textFields.Length] = slider.value.ToString();

        string path = "C:/User/Public/TestDataCollection/" + buildVersion + "/";

        try {
            if (!System.IO.Directory.Exists(path)) {
                System.IO.DirectoryInfo di = System.IO.Directory.CreateDirectory(path);
            } 
        }
        catch (System.Exception e) {
            Debug.LogError("Exeption: " + e);
            return;
        }

        
        string time = System.DateTime.Now.ToString();
        time = time.Replace('/', '_');
        time = time.Replace(':', '_');

        path += time + ", test number " + TestOder.ToString() + ".txt";
        System.IO.File.WriteAllLines(@path, data);

        if (SceneManager.sceneCountInBuildSettings > SceneManager.GetActiveScene().buildIndex + 1)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        else
            Application.Quit();
        

    }

}
