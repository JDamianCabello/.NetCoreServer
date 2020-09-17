using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Reflection;

namespace Serialization
{
    /// <summary>
    /// Class to Serialize/Deserialize objects
    /// 
    /// All server getting and sender are serializated so need this class
    /// </summary>
    class BinarySerialization
    {
        /// <summary>
        /// Serializate a object from memory
        /// </summary>
        /// <param name="toSerializate">Object to serilize</param>
        /// <returns></returns>
        public static byte[] Serializate(object toSerializate)
        {
            MemoryStream memory = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(memory, toSerializate);

            return memory.ToArray();
        }

        /// <summary>
        /// Deserializate a byte array into object
        /// </summary>
        /// <param name="data">Serializate data array</param>
        /// <returns></returns>
        public static object Deserializate(byte[] data)
        {
            MemoryStream memory = new MemoryStream(data);
            BinaryFormatter formatter = new BinaryFormatter
            {
                Binder = new CurrentAssemblyDeserializationBinder() //Change the current assambly to SharedNameSpace (All models have this namespace)
            };

            try
            {
                return formatter.Deserialize(memory);
            }
            catch (Exception)
            {
                memory.Dispose();
                return new byte[0];
            }
        }
    }

    /// <summary>
    /// Change the actual executing assambly for deserializate objects from other clients. If don´t have this will thrown SerializationError
    /// </summary>
    public class CurrentAssemblyDeserializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            return Type.GetType(string.Format("{0}, {1}", typeName, Assembly.GetExecutingAssembly().FullName));
        }
    }

}