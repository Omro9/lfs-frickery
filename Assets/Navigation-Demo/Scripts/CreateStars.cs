using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;


public class CreateStars : MonoBehaviour {
    public const int RADIUS = 700;

    public bool drawAll;
    public static GameObject star;

    private bool curDrawAllValue;

	void Start () {
        curDrawAllValue = drawAll;
        star = Resources.Load("Star") as GameObject;
        if (drawAll) {
            makeBSC();
        }
        else {
            //makeBigDipper();
            drawFromFile("north_star.txt");
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
        BinaryReader br = new BinaryReader(stream);        byte[] header = new byte[28];   // Bytes of BSC header section
        if (br.Read(header, 0, 28) != header.Length) { Debug.LogError("Failed to read header bytes to array"); return; }
        //        if (BitConverter.IsLittleEndian) {
        //            Array.Reverse(header);
        //        }
        int firstStarNum = BitConverter.ToInt32(header, 4); // Should be 1
        int bscSize = -BitConverter.ToInt32(header, 8);     // For some reason this value comes out negative. Should be 9110
        int entrySize = BitConverter.ToInt32(header, 24);   // Should be 32 bytes
        // Debug.Log(bscSize + "\t" + entrySize);

        for (int i = 1; i <= bscSize; ++i) {
            instantiateStar(getEntry(br, entrySize, i));
        }
        br.Close();
        stream.Close();
    }

    /*
     * Instantiates the stars constituting the big dipper
     */
    static void makeBigDipper() {
        TextAsset asset = Resources.Load("BSC5") as TextAsset;  // Loads BSC5.bytes from assets folder
        if (asset == null) { Debug.LogError("Failed to load asset"); return; }
        MemoryStream stream = new MemoryStream(asset.bytes);
        BinaryReader br = new BinaryReader(stream);        byte[] header = new byte[28];   // Bytes of BSC header section
        if (br.Read(header, 0, 28) != header.Length) { Debug.LogError("Failed to read header bytes to array"); return; }
//        if (BitConverter.IsLittleEndian) {
//            Array.Reverse(header);
//        }
        int firstStarNum = BitConverter.ToInt32(header, 4); // Should be 1
        int entrySize = BitConverter.ToInt32(header, 24);   // Should be 32

        // alpha uma: 4301
        byte[] alpha_uma = getEntry(br, entrySize, 4301);
        // beta uma: 4295
        byte[] beta_uma = getEntry(br, entrySize, 4295);
        // gamma uma: 4554
        byte[] gamma_uma = getEntry(br, entrySize, 4554);
        // delta uma: 4660
        byte[] delta_uma = getEntry(br, entrySize, 4660);
        // epsilon uma: 4905
        byte[] epsilon_uma = getEntry(br, entrySize, 4905);
        // zeta uma: 5054 or 5055
        byte[] zeta_uma = getEntry(br, entrySize, 5054);
        // eta uma: 5191
        byte[] eta_uma = getEntry(br, entrySize, 5191);

        byte[][] big_dipper = { alpha_uma, beta_uma, gamma_uma, delta_uma, epsilon_uma, zeta_uma, eta_uma };
        instantiateConstellation(big_dipper);
        stream.Close();
        br.Close();
    }

    /*
     * Returns bytes of BSC entry of given size with catalog number bscNum
     */
    static byte[] getEntry(BinaryReader br, int size, int bscNum) {
        byte[] entry = new byte[size];
        br.BaseStream.Seek(28 + (bscNum - 1) * size, SeekOrigin.Begin);
        if (br.Read(entry, 0, size) != size) {
            Debug.LogError("Failed to read all bytes of entry " + bscNum);
            return null;
        }
        // if (BitConverter.IsLittleEndian) {
        //      Array.Reverse(entry);
        // }
        if (Math.Abs(BitConverter.ToSingle(entry, 0) - bscNum) > 1) {   // since numbers are floats, being safe and checking difference rather than equality
            Debug.LogError("Failed to read entry " + bscNum);
            return null;
        }
        return entry;
    }

    /*
     * Repeatedly calls instantiateStar on the given array of BSC entries.
     */
    static void instantiateConstellation(byte [][] entries) {
        for (int i = 0; i < entries.Length; ++i) {
            instantiateStar(entries[i]).transform.localScale = new Vector3(5, 5, 5);
        }
    }

    /*
     * Creates an instance of the given star at it's correct right ascension and declination
     * CreateStars.RADIUS units away from the origin.
     */
    static GameObject instantiateStar(byte [] entry) {
        double ra = BitConverter.ToDouble(entry, 4) + Math.PI / 2D; // RA is off by 90 deg compared to skybox texture
        double dec = BitConverter.ToDouble(entry, 12);
        float mag = BitConverter.ToInt16(entry, 22) / 100.0F;

        float x, y, z;
        x = (float)(RADIUS * Math.Cos(dec) * Math.Cos(ra));
        y = (float)(RADIUS * Math.Cos(dec) * Math.Sin(ra));
        z = (float)(RADIUS * Math.Sin(dec));

        GameObject instance = Instantiate(star);
        instance.transform.position = new Vector3(x, z, y); // x, y, and z are mathematical axes, not Unity's
        instance.transform.localScale = instance.transform.localScale * mag;
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
