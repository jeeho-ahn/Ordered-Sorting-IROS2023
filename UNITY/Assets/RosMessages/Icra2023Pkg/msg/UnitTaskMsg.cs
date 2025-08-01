//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Icra2023Pkg
{
    [Serializable]
    public class UnitTaskMsg : Message
    {
        public const string k_RosMessageName = "icra2023_pkg/UnitTask";
        public override string RosMessageName => k_RosMessageName;

        public Std.Int32Msg robot_id;
        public Std.StringMsg task_name;
        public TargetMsgMsg target;

        public UnitTaskMsg()
        {
            this.robot_id = new Std.Int32Msg();
            this.task_name = new Std.StringMsg();
            this.target = new TargetMsgMsg();
        }

        public UnitTaskMsg(Std.Int32Msg robot_id, Std.StringMsg task_name, TargetMsgMsg target)
        {
            this.robot_id = robot_id;
            this.task_name = task_name;
            this.target = target;
        }

        public static UnitTaskMsg Deserialize(MessageDeserializer deserializer) => new UnitTaskMsg(deserializer);

        private UnitTaskMsg(MessageDeserializer deserializer)
        {
            this.robot_id = Std.Int32Msg.Deserialize(deserializer);
            this.task_name = Std.StringMsg.Deserialize(deserializer);
            this.target = TargetMsgMsg.Deserialize(deserializer);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.robot_id);
            serializer.Write(this.task_name);
            serializer.Write(this.target);
        }

        public override string ToString()
        {
            return "UnitTaskMsg: " +
            "\nrobot_id: " + robot_id.ToString() +
            "\ntask_name: " + task_name.ToString() +
            "\ntarget: " + target.ToString();
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
