//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Icra2023Pkg
{
    [Serializable]
    public class robotTargetSrvRequest : Message
    {
        public const string k_RosMessageName = "icra2023_pkg/robotTargetSrv";
        public override string RosMessageName => k_RosMessageName;

        public RobotTargetPairMsg[] pair;

        public robotTargetSrvRequest()
        {
            this.pair = new RobotTargetPairMsg[0];
        }

        public robotTargetSrvRequest(RobotTargetPairMsg[] pair)
        {
            this.pair = pair;
        }

        public static robotTargetSrvRequest Deserialize(MessageDeserializer deserializer) => new robotTargetSrvRequest(deserializer);

        private robotTargetSrvRequest(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.pair, RobotTargetPairMsg.Deserialize, deserializer.ReadLength());
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.WriteLength(this.pair);
            serializer.Write(this.pair);
        }

        public override string ToString()
        {
            return "robotTargetSrvRequest: " +
            "\npair: " + System.String.Join(", ", pair.ToList());
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize);
        }
    }
}
