using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using System.IO;

public class CaptureDevice : MonoBehaviour
{
    public enum usageMode
    {
        train,
        run
    }

    public usageMode mode;
    public bool enableWrite;
    public bool initOnStart;
    public int writeFrequency;
    public int startIndex;
    public bool debugValue;
    public bool autoClose;

    Thread mThread;
    public string connectionIP = "127.0.0.1";
    public int connectionPort = 25001;
    IPAddress localAddress;
    TcpListener listener;
    TcpClient client;

    private bool running;

    private int c;
    private StreamWriter writer;
    private carController cc;
    private TrackBuilder track;
    private string write_path = "Assets/CAS_trainingData";
    private int i;
    private float progress;

    public float cnn_pred { get; set; }

    void Start()
    {
        if (mode == usageMode.train)
        {
            c = startIndex;
            track = GetComponent<CarAiController>().trackBuilder;
            if (initOnStart)
                writer = new StreamWriter(write_path + "/track_data_temp.csv");
            cc = FindObjectOfType<carController>();
        }
        else
        {
            ThreadStart ts = new ThreadStart(GetInfo);
            mThread = new Thread(ts);
            mThread.Start();
        }
    }

    void Update()
    {
        if (mode == usageMode.train)
        {
            progress = track.GetTrackProgress(this.gameObject);
            if (debugValue && Time.frameCount % writeFrequency == 0)
                Debug.Log("Steer Angle = " + cc.steerAngle + ", write count = " + i++ + ", progress = " + progress + "%");

            if (initOnStart && enableWrite && Time.frameCount % writeFrequency == 0)
            {
                ScreenCapture.CaptureScreenshot(write_path + "/CameraCapture/Capture_" + c + ".png");
                writer.Write(cc.steerAngle + "\n");
                c++;
            }

            if (progress >= 99 && autoClose)
                UnityEditor.EditorApplication.isPlaying = false;
        }
        else
        {
            if (Time.frameCount % writeFrequency == 0)
            {
                ScreenCapture.CaptureScreenshot("F:/Study/Machine Learning/Projects/cnn_regressor/test_image.png");
            }

        }

    }

    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system");
    }

    void GetInfo()
    {
        localAddress = IPAddress.Parse(connectionIP);
        listener = new TcpListener(IPAddress.Any, connectionPort);
        listener.Start();

        client = listener.AcceptTcpClient();

        running = true;
        while (running)
        {
            Connection();
        }
        listener.Stop();
    }

    void Connection()
    {
        NetworkStream nwStream = client.GetStream();
        byte[] buffer = new byte[client.ReceiveBufferSize];

        int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);
        string dataRecieved = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        if (dataRecieved != null)
        {
            if (dataRecieved == "stop")
                running = false;
            else
            {
                cnn_pred = float.Parse(dataRecieved);
                Debug.Log(cnn_pred);
            }
        }
    }

    private void OnApplicationQuit()
    {
        if (writer != null)
            writer.Close();
    }
}