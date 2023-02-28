using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileWriterScript
{
    private string foldername = "DebugLogs\\";
    public string writeDebugTxt(Landscape l, GraphRendererScript g)
    {
        string filename = "Debug_log_" +
            "Year" + System.DateTime.Now.Year + "_" +
            "Month" + System.DateTime.Now.Month + "_" +
            "Day" + System.DateTime.Now.Day + "_" +
            "Hour" + System.DateTime.Now.Hour + "_" +
            "Min" + System.DateTime.Now.Minute + "_" +
            "Sec" + System.DateTime.Now.Second + "_" +
            "Milli" + System.DateTime.Now.Millisecond +
            ".txt";
        
        this.writeDebugTxt(filename,l, g);
        return filename;
    }
    
    public void writeDebugTxt(string filename, Landscape l, GraphRendererScript g)
    {
        System.IO.StreamWriter writer = new System.IO.StreamWriter(foldername + filename, true);

        g.recalculateGraphProperties();
        l.sortAgents();
        writer.Write("\n<<Simulation Parameters>>\n" + JsonUtility.ToJson(Parameters.Instance, true) + "\n\n");
        writer.Write("\n<<Graph Renderer>>\n" + g.toTxt() + "\n\n");
        writer.Write("\n<<Landscape>>\n" + l.toTxt() + "\n\n");
        writer.Close();
    }
}
