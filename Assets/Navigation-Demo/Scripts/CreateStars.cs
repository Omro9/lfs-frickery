using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

/*
 * Script to instantiate given stars from Yale Bright Star Catalog (BSC5.bytes in Resources folder)
 */
public class CreateStars : MonoBehaviour {
    public bool drawAll;
    public static GameObject star;

    private static GameObject starParent;
    private bool curDrawAllValue;

	void Start () {
        starParent = new GameObject("Star Parent Object");
        curDrawAllValue = drawAll;
        star = Resources.Load("Star") as GameObject;
        if (drawAll) {
            makeBSC();
        }
        else {
            drawFromFile("big_dipper.txt");
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (curDrawAllValue != drawAll) {
            cleanStars();
            Start();
        }
	}

    /*
     * Instantiates all 9110 stars in the Yale Bright Star Catalog
     */
    static void makeBSC() {
        TextAsset asset = Resources.Load("BSC5") as TextAsset;  // Loads BSC5.bytes from assets folder
        if (asset == null) { Debug.LogError("Failed to load asset"); return; }
        MemoryStream stream = new MemoryStream(asset.bytes);
        BinaryReader br = new BinaryReader(stream);
        byte[] header = new byte[28];   // Bytes of BSC header section
        if (br.Read(header, 0, 28) != header.Length) { Debug.LogError("Failed to read header bytes to array"); return; }
        //        if (BitConverter.IsLittleEndian) {        // This code seems to be unnecessary and breaks the script
        //            Array.Reverse(header);
        //        }
        int firstStarNum = BitConverter.ToInt32(header, 4); // Catalog number of first star in catalog. Should be 1
        int bscSize = -BitConverter.ToInt32(header, 8);     // Number of stars in the BSC. For some reason this value comes out negative. Should be 9110
        int entrySize = BitConverter.ToInt32(header, 24);   // Size in bytes of an entry in the BSC. Should be 32
        // Debug.Log(bscSize + "\t" + entrySize);

        for (int i = 1; i <= bscSize; ++i) {
            instantiateStar(getEntry(br, entrySize, i));
        }
        br.Close();
        stream.Close();
    }

    /*
     * Returns bytes of BSC entry of given size with catalog number bscNum
     */
    static byte[] getEntry(BinaryReader rdr, int entrySize, int bscNum) {
        byte[] entry = new byte[entrySize];
        rdr.BaseStream.Seek(28 + (bscNum - 1) * entrySize, SeekOrigin.Begin); // Move reader to position of entry number bscNum
        if (rdr.Read(entry, 0, entrySize) != entrySize) {
            Debug.LogError("Failed to read all bytes of entry " + bscNum);
            return null;
        }
        // if (BitConverter.IsLittleEndian) {
        //      Array.Reverse(entry);
        // }
        if (Math.Abs(BitConverter.ToSingle(entry, 0) - bscNum) > 1) {   // Since numbers are floats, being safe and checking difference rather than equality
            Debug.LogError("Failed to read entry " + bscNum);           // (I don't know if this actually matters but Visual Studio complains otherwise)
            return null;
        }
        return entry;
    }

    /*
     * Creates an instance of the given star at it's correct right ascension and declination
     * CreateStars.RADIUS units away from the origin.
     */
    static GameObject instantiateStar(byte [] entry) {
        double ra = BitConverter.ToDouble(entry, 4) + Math.PI / 2D; // Get right ascension. RA is off by 90 deg compared to skybox texture
        double dec = BitConverter.ToDouble(entry, 12);              // Get declination
        float mag = BitConverter.ToInt16(entry, 22) / 100.0F;       // Get brightness magnitude

        float x, z, y;                                              // Convert RA and DEC into xyz coordinates
        x = (float)(AnimateStar.RADIUS * Math.Cos(dec) * Math.Cos(ra));
        z = (float)(AnimateStar.RADIUS * Math.Cos(dec) * Math.Sin(ra));
        y = (float)(AnimateStar.RADIUS * Math.Sin(dec));

        GameObject instance = Instantiate(star);                    // Instantiate star with correct init fields
        instance.GetComponent<AnimateStar>().luminance = mag;
        instance.GetComponent<AnimateStar>().position = new Vector3(x, y, z);
        instance.transform.SetParent(starParent.transform);
        return instance;
    }

    static void drawFromFile(string filename) {
        StreamReader rdr = new StreamReader("Assets/Resources/" + filename);
        if (rdr == null) { Debug.LogError("Failed to load file " + filename); }
        int fileLength = Int32.Parse(rdr.ReadLine());
        int[] catalogNumbers = new int[fileLength];
        for (int i = 0; i < fileLength; ++i) {
            catalogNumbers[i] = Int32.Parse(rdr.ReadLine());
        }
        rdr.Close();

        TextAsset asset = Resources.Load("BSC5") as TextAsset;  // Loads BSC5.bytes from assets folder
        if (asset == null) { Debug.LogError("Failed to load asset"); return; }
        MemoryStream stream = new MemoryStream(asset.bytes);
        BinaryReader br = new BinaryReader(stream);
        byte[] header = new byte[28];   // Bytes of BSC header section
        if (br.Read(header, 0, 28) != header.Length) { Debug.LogError("Failed to read header bytes to array"); return; }
        //        if (BitConverter.IsLittleEndian) {
        //            Array.Reverse(header);
        //        }
        int firstStarNum = BitConverter.ToInt32(header, 4); // Should be 1
        int entrySize = BitConverter.ToInt32(header, 24);   // Should be 32
        for (int i = 0; i < catalogNumbers.Length; ++i) {
            instantiateStar(getEntry(br, entrySize, catalogNumbers[i])).transform.localScale *= 5F;
        }

        stream.Close();
        br.Close();
    }

    static void cleanStars() {
        GameObject instance;
        while ((instance = GameObject.Find("Star(Clone)")) != null) {
            Destroy(instance);
        }
    }

}
