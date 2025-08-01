//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Icra2023Pkg
{
    [Serializable]
    public class MobileRobotPoseArrayMsg : Message
    {
        public const string k_RosMessageName = "icra2023_pkg/MobileRobotPoseArray";
        public override string RosMessageName => k_RosMessageName;

        public MobileRobotPoseMsg[] poses;

        public MobileRobotPoseArrayMsg()
        {
            this.poses = new MobileRobotPoseMsg[0];
        }

        public MobileRobotPoseArrayMsg(MobileRobotPoseMsg[] poses)
        {
            this.poses = poses;
        }

        public static MobileRobotPoseArrayMsg Deserialize(MessageDeserializer deserializer) => new MobileRobotPoseArrayMsg(deserializer);

        private MobileRobotPoseArrayMsg(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.poses, MobileRobotPoseMsg.Deserialize, deserializer.ReadLength());
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.WriteLength(this.poses);
            serializer.Write(this.poses);
        }

        public override string ToString()
        {
            return "MobileRobotPoseArrayMsg: " +
            "\nposes: " + System.String.Join(", ", poses.ToList());
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
