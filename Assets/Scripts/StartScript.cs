using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XnaFlashPlayer;
//using UnityEngine.PSVita;

public class StartScript : MonoBehaviour
{
    MainForm form;
    public Slider slider;

    public DrawGL DrawGL;
    public Vector3 Pos;
    
    public float OrthographicSize = 0;
    public Camera MainCamera;
    public Font font;
    public Unity.API.UnityWinForms WinForms;
    public static StartScript Instance;
    public static int Id;
    public bool isPlay;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (WinForms == null)
        {
            WinForms= gameObject.AddComponent<Unity.API.UnityWinForms>();
            WinForms.Resources.Fonts.Add(font);
        }
        Unload();
        form = new MainForm();
        form.ShowMainMenu();
        form.Show();
        form.ToolStripMenuItem_Click(null, null);
        //form.Hide();
        Pos = Camera.main.transform.position;
        //UnityEngine.PSVita.Diagnostics.enableHUD = true;
        OrthographicSize  = MainCamera.orthographicSize;
        slider.onValueChanged.AddListener((v) => {
            transform.localScale = -v*Vector3.one;
            float vv = v * PosMoveValue;
            //transform.position = new Vector3(vv- PosMoveValue, vv - PosMoveValue, 0);
        });
        slider.gameObject.SetActive(false);
    }
    public float PosMoveValue = 2000;

    void Unload()
    {
        //if(UnityEngine.PSVita.Diagnostics.GetFreeMemoryCDRAM()<10)
        System.GC.Collect();
        Resources.UnloadUnusedAssets();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O) || Input.GetKeyDown(KeyCode.JoystickButton2))
        {
            Application.LoadLevel(Application.loadedLevelName);
        }

        if (Input.GetKeyDown(KeyCode.P)|| Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            form.Play();
            RefreshLocalScale();
        }

        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.JoystickButton6))
        {
            form.SetPlay();
        }

        if (DrawGL.FileNameText.text == "")
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.JoystickButton11))
            {
                form.L();
            }

            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.JoystickButton9))
            {
                form.R();
            }

            else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.JoystickButton8))
            {
                form.Up();
            }

            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.JoystickButton10))
            {
                form.Down();
            }

            else if (Input.GetKeyDown(KeyCode.H) || Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                form.OpenFile();
            }
        }
           

        else if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.JoystickButton4))
        {
            form.předchozíSnímekToolStripMenuItem_Click(null,null);
        }
        else if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.JoystickButton5))
        {
            form.dalšíSnímekToolStripMenuItem_Click(null, null);
        }

        if (Input.GetKeyDown(KeyCode.G))
            RefreshLocalScale();
        Pyv = Py * Pv;
        Pxv = Px * Pv;
    }

    public void RefreshLocalScale()
    {
        W = XnaFlash.FlashDocument.WH.X;
        H = XnaFlash.FlashDocument.WH.Y;
        Py =  YValue/H;
        XValue = W * Py;
        //Px =2- W/XValue;
        Px = Py;

        transform.localScale = Vector3.one * -Py;
       
        slider.gameObject.SetActive(true);
        slider.maxValue = Py + 1;
        slider.minValue = Py;
        slider.value = Py;
        X *= (2-Py);
        //Y *= Py;
        PosMoveValue /= Py;
    }
    public float YValue = 8200;
    public float H;
    public float XValue = 11400;
    public float W;
    [HideInInspector] public float Pyv;
    [HideInInspector] public float Pxv;
    [HideInInspector] public float Py;
    [HideInInspector] public float Px;
    public float Pv=0.02f;
    public float X = 180;
    public float Y = 390;

    public bool UseCurved;
}
