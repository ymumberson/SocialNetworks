using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileWriterScript
{
    private string foldername = "DebugLogs\\";
    private string CSV_NAME = "1_Output.csv";

    public string writeDebugTxt(Landscape l, GraphRendererScript g)
    {
        string filename =
            Parameters.Instance.TEST_NAME.Replace(" ", "-") + "(" +
            System.DateTime.Now.Year + "-" +
            System.DateTime.Now.Month + "-" +
            System.DateTime.Now.Day + "-" +
            System.DateTime.Now.Hour + "-" +
            System.DateTime.Now.Minute + "-" +
            System.DateTime.Now.Second +
            ").txt";

        this.writeDebugTxt(filename,l, g);
        return filename;
    }
    
    public void writeDebugTxt(string filename, Landscape l, GraphRendererScript g)
    {
        System.IO.StreamWriter writer = new System.IO.StreamWriter(foldername + filename, true);

        g.recalculateGraphProperties();
        g.calculateIdealNeighbours();
        g.calculateIdealGraphProperties();
        l.sortAgents();
        writer.Write("\n<<Graph Properties>>\n" + g.toTxt() + "\n\n");
        writer.Write("\n<<Ideal Graph Properties>>\n" + g.toTxtIdeal() + "\n\n");
        writer.Write("\n<<Simulation Parameters>>\n" + JsonUtility.ToJson(Parameters.Instance, true) + "\n\n");
        writer.Write("\n<<Landscape Controller>>\n" + l.toTxt() + "\n\n");
        writer.Close();

        addToCSV(l, g);
    }

    public void addToCSV(Landscape l, GraphRendererScript g)
    {
        string filename = foldername + CSV_NAME;
        bool writeAttributeNames = !System.IO.File.Exists(filename); 
        System.IO.StreamWriter writer = new System.IO.StreamWriter(filename, true);
        if (writeAttributeNames)
        {
            writer.Write(Parameters.Instance.titlesCSV() + g.titlesCSV() + g.titlesCSVIdeal() + "\n");
        }
        writer.Write(Parameters.Instance.toCSV() + g.toCSV() + g.toCSVIdeal() + "\n");
        writer.Close();
    }
}
