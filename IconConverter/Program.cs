using System.IO;

class Program {
    static void Main() {
        byte[] pngData = File.ReadAllBytes(@"e:\PredatorRGB\logo.png");
        using (var fs = new FileStream(@"e:\PredatorRGB\logo.ico", FileMode.Create))
        using (var bw = new BinaryWriter(fs)) {
            // ICO Header
            bw.Write((short)0); // reserved
            bw.Write((short)1); // type (1 = ico)
            bw.Write((short)1); // image count
            
            // Image Directory
            bw.Write((byte)0); // width (0 = 256)
            bw.Write((byte)0); // height (0 = 256)
            bw.Write((byte)0); // color count
            bw.Write((byte)0); // reserved
            bw.Write((short)1); // color planes
            bw.Write((short)32); // bits per pixel
            bw.Write((int)pngData.Length); // size of image data
            bw.Write((int)22); // offset of image data
            
            // Image Data
            bw.Write(pngData);
        }
    }
}
