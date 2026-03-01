//using DPUruNet;
using System.Text;

namespace RVMSService.Helpers
{
    public static class FmdSerializer
    {
        //public static byte[] Serialize(Fmd fmd)
        //{
        //    var fmdBytes = fmd.Bytes;
        //    var versionBytes = Encoding.UTF8.GetBytes(fmd.Version ?? string.Empty);

        //    // 4 (format) + 4 (version length) + version + fmd data
        //    var result = new byte[4 + 4 + versionBytes.Length + fmdBytes.Length];

        //    BitConverter.GetBytes((int)fmd.Format).CopyTo(result, 0);
        //    BitConverter.GetBytes(versionBytes.Length).CopyTo(result, 4);
        //    versionBytes.CopyTo(result, 8);
        //    fmdBytes.CopyTo(result, 8 + versionBytes.Length);

        //    return result;
        //}

        //public static Fmd Deserialize(byte[] data)
        //{
        //    int format = BitConverter.ToInt32(data, 0);
        //    int versionLen = BitConverter.ToInt32(data, 4);
        //    string version = Encoding.UTF8.GetString(data, 8, versionLen);

        //    int fmdOffset = 8 + versionLen;
        //    int fmdLength = data.Length - fmdOffset;
        //    var fmdBytes = new byte[fmdLength];
        //    Array.Copy(data, fmdOffset, fmdBytes, 0, fmdLength);

        //    return new Fmd(fmdBytes, format, version);
        //}

       
            /// <summary>Stores a native SDK FMD byte array into the database column.</summary>
            public static byte[] Serialize(byte[] fmdData) => fmdData;

            /// <summary>Retrieves a native SDK FMD byte array from the database column.</summary>
            public static byte[] Deserialize(byte[] data) => data;
        
    }
}
