//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Icra2023Pkg
{
    [Serializable]
    public class robotEventsSrvResponse : Message
    {
        public const string k_RosMessageName = "icra2023_pkg/robotEventsSrv";
        public override string RosMessageName => k_RosMessageName;

        public UnitTaskMsg[] new_tasks;

        public robotEventsSrvResponse()
        {
            this.new_tasks = new UnitTaskMsg[0];
        }

        public robotEventsSrvResponse(UnitTaskMsg[] new_tasks)
        {
            this.new_tasks = new_tasks;
        }

        public static robotEventsSrvResponse Deserialize(MessageDeserializer deserializer) => new robotEventsSrvResponse(deserializer);

        private robotEventsSrvResponse(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.new_tasks, UnitTaskMsg.Deserialize, deserializer.ReadLength());
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.WriteLength(this.new_tasks);
            serializer.Write(this.new_tasks);
        }

        public override string ToString()
        {
            return "robotEventsSrvResponse: " +
            "\nnew_tasks: " + System.String.Join(", ", new_tasks.ToList());
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize, MessageSubtopic.Response);
        }
    }
}
