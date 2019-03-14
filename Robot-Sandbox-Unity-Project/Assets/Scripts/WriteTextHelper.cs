using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// ========================= //
//  Local File Manipulation  //
// ========================= //

public class WriteTextHelper : MonoBehaviour {

    // ---------------------------------------------------------------------
    // Cached Reference
    // ---------------------------------------------------------------------

    private InteractionHelper interactionHelper = new InteractionHelper(); 

    // ---------------------------------------------------------------------
    // Writing coordinates data into .txt file
    // ---------------------------------------------------------------------
    public void WriteString(
        string string1,
        string string2,
        string string3,
        string string4,
        string string5,
        string string6) {
        // need to re-assign the path variable or otherwise will encounter ArgumentNullException
        interactionHelper.path = "C:/Users/HRK/Documents/DanRoboticsBricks/test.txt";

        // create the writer object instance
        StreamWriter writer = new StreamWriter(interactionHelper.path, true);

        // Write the position and rotation information to the test.txt file
        writer.WriteLine(
            string1 + "," +
            string2 + "," +
            string3 + "," +
            string4 + "," +
            string5 + "," +
            string6 + ",");

        writer.Close();
    }

    // a more robust and well-justified way
    public void WriteString2(
        string string1,
        string string2,
        string string3,
        string string4,
        string string5,
        string string6) {
        interactionHelper.path = "C:/Users/HRK/Documents/DanRoboticsBricks/test.txt";

        // create the stream before making the writer
        using (var stream = new FileStream(interactionHelper.path, FileMode.OpenOrCreate, FileAccess.Write)) {
            var writer = new StreamWriter(stream, System.Text.Encoding.UTF8);

            writer.WriteLine(
                string1 + "," +
                string2 + "," +
                string3 + "," +
                string4 + "," +
                string5 + "," +
                string6 + ",");

            writer.Flush();
            writer.Dispose();
        }
    }

    // ---------------------------------------------------------------------
    // Reading coordinates data from .txt file
    // ---------------------------------------------------------------------
    public void ReadString() {
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(interactionHelper.path);
        Debug.Log(reader.ReadToEnd());
        reader.Close();
    }
}