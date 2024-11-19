using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MiniChat.Transmitting
{
    public class BytesConvert
    {
        public static byte[] ObjectToBytes(object obj)
        {
            try
            {
                // 将对象序列化为 JSON 字节数组
                return JsonSerializer.SerializeToUtf8Bytes(obj, obj.GetType());
            }
            catch (Exception ex)
            {
                // 抛出自定义的 SerializationException 异常
                throw new SerializationException("Error serializing object to bytes.", ex);
            }
        }
        public static object BytesToObject(byte[] bytes, int effectiveByte, string namespaceName)
        {
                try
                {
                    // 将字节数组截取为有效字节范围
                    ReadOnlySpan<byte> jsonSpan = new ReadOnlySpan<byte>(bytes, 0, effectiveByte);

                    // 反序列化为 Transmit 类型的对象
                    return JsonSerializer.Deserialize<Transmit>(jsonSpan);
                }
                catch (JsonException jex)
                {
                // 抛出自定义的 SerializationException 异常
                throw new SerializationException("JSON exception occurred during deserialization.", jex);
            }
            catch (Exception ex)
            {
                // 抛出自定义的 SerializationException 异常
                throw new SerializationException("Error occurred during deserialization.", ex);
            }
        }
     }
}
