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
                // ���������л�Ϊ JSON �ֽ�����
                return JsonSerializer.SerializeToUtf8Bytes(obj, obj.GetType());
            }
            catch (Exception ex)
            {
                // �׳��Զ���� SerializationException �쳣
                throw new SerializationException("Error serializing object to bytes.", ex);
            }
        }
        public static object BytesToObject(byte[] bytes, int effectiveByte, string namespaceName)
        {
                try
                {
                    // ���ֽ������ȡΪ��Ч�ֽڷ�Χ
                    ReadOnlySpan<byte> jsonSpan = new ReadOnlySpan<byte>(bytes, 0, effectiveByte);

                    // �����л�Ϊ Transmit ���͵Ķ���
                    return JsonSerializer.Deserialize<Transmit>(jsonSpan);
                }
                catch (JsonException jex)
                {
                // �׳��Զ���� SerializationException �쳣
                throw new SerializationException("JSON exception occurred during deserialization.", jex);
            }
            catch (Exception ex)
            {
                // �׳��Զ���� SerializationException �쳣
                throw new SerializationException("Error occurred during deserialization.", ex);
            }
        }
     }
}
