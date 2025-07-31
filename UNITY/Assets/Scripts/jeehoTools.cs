using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class jeehoTools : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //var m = transform.worldToLocalMatrix;
        //var m2 = transform.localToWorldMatrix;
        //var m3 = m * m2;
        //UnityEngine.Debug.Log(m3.ToString());

        /*
        var m1 = new Matrix4x4();
        m1.SetRow(0, new Vector4(-0.0203f, 0.4828f, 0.8755f, -0.6402f));
        m1.SetRow(1, new Vector4(0.5436f, -0.7296f, 0.4149f, -0.1597f));
        m1.SetRow(2, new Vector4(0.8391f,0.4843f, -0.2476f,0.8097f));
        m1.SetRow(3, new Vector4(0,0,0,1));

        var m2 = new Matrix4x4();
        m2.SetRow(0, new Vector4(0.5881f,   0.4130f, -0.6954f, -0.7969f));
        m2.SetRow(1, new Vector4(0.1747f,   0.7746f,   0.6078f,   0.5132f));
        m2.SetRow(2, new Vector4(0.7897f, -0.4790f,   0.3834f, -0.5390f));
        m2.SetRow(3, new Vector4(0, 0, 0, 1));

        var m3 = m1 * m2;
        UnityEngine.Debug.Log(m3.ToString());

        var m4 = m2 * m1;
        UnityEngine.Debug.Log(m4.ToString());
        */

        /*
        var rx = 0.785398f;
        var ry = 0;
        var rz = 0.785398f;

        var mat = rotz(rz) * roty(ry) * rotx(rx);

        UnityEngine.Debug.Log(mat.ToString());
        */
    }

    public static Matrix4x4 eul2R(Vector3 euler, string order = "ZYX")
    {
        if (order == "ZYX")
            return rotz(euler.z) * roty(euler.y) * rotx(euler.x);
        else if(order == "XYZ")
            return rotx(euler.x) * roty(euler.y) * rotz(euler.z);
        //todo: add the 10 other orders
        else
        {
            UnityEngine.Debug.LogWarning("Rotation order not valid: " + order);
        }
        return Matrix4x4.identity;
    }

    // Calculates rotation matrix to euler angles
    // The result is the same as MATLAB except the order
    // of the euler angles ( x and z are swapped ).
    public static Vector3 rotationMatrixToEulerAngles(Matrix4x4 R)
    {
        //assert(isRotationMatrix(R));
        float sy = Mathf.Sqrt(R[0, 0] * R[0, 0] + R[1, 0] * R[1, 0]);
        bool singular = sy < 1e-6; // If
        float x, y, z;
        if (!singular)
        {
            x = Mathf.Atan2(R[2, 1], R[2, 2]);
            y = Mathf.Atan2(-1*R[2, 0], sy);
            z = Mathf.Atan2(R[1, 0], R[0, 0]);
        }
        else
        {
            x = Mathf.Atan2(-1*R[1, 2], R[1, 1]);
            y = Mathf.Atan2(-1*R[2, 0], sy);
            z = 0;
        }
        return new Vector3(x, y, z);
    }

    public static Matrix4x4 unityH2rosH(Matrix4x4 H_unity)
    {
        //decompose to quaternion and translation
        var q_u = QFromMat_unity(H_unity);
        var t4_u = H_unity.GetColumn(3);
        var t_u = new Vector3(t4_u.x, t4_u.y, t4_u.z);

        var q_r = ros_conversions.unity2ros(q_u);
        var t_r = ros_conversions.unity2ros(t_u);

        var H_out = quatToMatrix(q_r);
        H_out.SetColumn(3, new Vector4(t_r.x, t_r.y, t_r.z,1));

        return H_out;
    }

    public static Matrix4x4 quatToMatrix(Quaternion q)
    {
        float sqw = q.w * q.w;
        float sqx = q.x * q.x;
        float sqy = q.y * q.y;
        float sqz = q.z * q.z;

        // invs (inverse square length) is only required if quaternion is not already normalised
        float invs = 1 / (sqx + sqy + sqz + sqw);
        float m00 = (sqx - sqy - sqz + sqw) * invs; // since sqw + sqx + sqy + sqz =1/invs*invs
        float m11 = (-sqx + sqy - sqz + sqw) * invs;
        float m22 = (-sqx - sqy + sqz + sqw) * invs;

        float tmp1 = q.x * q.y;
        float tmp2 = q.z * q.w;
        float m10 = 2.0f * (tmp1 + tmp2) * invs;
        float m01 = 2.0f * (tmp1 - tmp2) * invs;

        tmp1 = q.x * q.z;
        tmp2 = q.y * q.w;
        float m20 = 2.0f * (tmp1 - tmp2) * invs;
        float m02 = 2.0f * (tmp1 + tmp2) * invs;
        tmp1 = q.y * q.z;
        tmp2 = q.x * q.w;
        float m21 = 2.0f * (tmp1 + tmp2) * invs;
        float m12 = 2.0f * (tmp1 - tmp2) * invs;

        Matrix4x4 out_mat = Matrix4x4.identity;
        out_mat.SetRow(0, new Vector4(m00, m01, m02));
        out_mat.SetRow(1, new Vector4(m10, m11, m12));
        out_mat.SetRow(2, new Vector4(m20, m21, m22));

        return out_mat;
    }


    public static Quaternion QFromMat_unity(Matrix4x4 m)
    {
        /*
        // Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
        Quaternion q = new Quaternion();
        q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2])) / 2;
        q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2])) / 2;
        q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2])) / 2;
        q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2])) / 2;
        q.x *= Mathf.Sign(q.x * (m[2, 1] - m[1, 2]));
        q.y *= Mathf.Sign(q.y * (m[0, 2] - m[2, 0]));
        q.z *= Mathf.Sign(q.z * (m[1, 0] - m[0, 1]));
        return q;
        */
        // adapted from http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm

        Matrix4x4 matrix = new Matrix4x4();
        matrix.SetColumn(0, m.GetColumn(0));
        matrix.SetColumn(1, m.GetColumn(1));
        matrix.SetColumn(2, m.GetColumn(2));
        matrix.SetColumn(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));

        float trace = m[0,0]+m[1,1]+m[2,2];
        if (trace > 0.0f)
        {
            //float s = Mathf.Sqrt(trace + 1.0f) * 2.0f; // 4 * qw in Jon's equ
            float s = 0.5f / Mathf.Sqrt(trace + 1.0f);
            return new Quaternion(
                (matrix[2, 1] - matrix[1, 2]) / s,
                (matrix[0, 2] - matrix[2, 0]) / s,
                (matrix[1, 0] - matrix[0, 1]) / s,
                0.25f * s);
        }
        else if (matrix[0, 0] > matrix[1, 1] && matrix[0, 0] > matrix[2, 2])
        {
            float s = Mathf.Sqrt(1.0f + matrix[0, 0] - matrix[1, 1] - matrix[2, 2]) * 2.0f; // 4 * qx
            return new Quaternion(
                0.25f * s,
                (matrix[0, 1] + matrix[1, 0]) / s,
                (matrix[0, 2] + matrix[2, 0]) / s,
                (matrix[2, 1] - matrix[1, 2]) / s);
        }
        else if (matrix[1, 1] > matrix[2, 2])
        {
            float s = Mathf.Sqrt(1.0f + matrix[1, 1] - matrix[0, 0] - matrix[2, 2]) * 2.0f; // 4 * qw
            return new Quaternion(
                (matrix[0, 1] + matrix[1, 0]) / s,
                0.25f * s,
                (matrix[1, 2] + matrix[2, 1]) / s,
                (matrix[0, 2] - matrix[2, 0]) / s);
        }
        else
        {
            float s = Mathf.Sqrt(1.0f + matrix[2, 2] - matrix[0, 0] - matrix[1, 1]) * 2.0f; // 4 * qz
            return new Quaternion(
                (matrix[0, 2] + matrix[2, 0]) / s,
                (matrix[1, 2] + matrix[2, 1]) / s,
                0.25f * s,
                (matrix[1, 0] - matrix[0, 1]) / s);
        }
    }

    static Matrix4x4 rotx(float theta, bool is_rad = true)
    {
        if (!is_rad)
            theta *= Mathf.Deg2Rad;
     //        | 1      0           0 |
     //    T = | 0  cos(angle) - sin(angle) |
     //        | 0  sin(angle)  cos(angle) |
        var m_out = new Matrix4x4();
        m_out.SetRow(0, new Vector4(1, 0, 0, 0));
        m_out.SetRow(1, new Vector4(0, Mathf.Cos(theta), -1*Mathf.Sin(theta), 0));
        m_out.SetRow(2, new Vector4(0, Mathf.Sin(theta), Mathf.Cos(theta), 0));
        m_out.SetRow(3, new Vector4(0, 0, 0, 1));

        return m_out;
    }

    static Matrix4x4 roty(float theta, bool is_rad = true)
    {
        if (!is_rad)
            theta *= Mathf.Deg2Rad;
      //      | cos(angle)  0  sin(angle) |
      //  T = | 0       1      0 |
      //      | -sin(angle)  0  cos(angle) |
        var m_out = new Matrix4x4();
        m_out.SetRow(0, new Vector4(Mathf.Cos(theta), 0, Mathf.Sin(theta), 0));
        m_out.SetRow(1, new Vector4(0,1,0, 0));
        m_out.SetRow(2, new Vector4(-1*Mathf.Sin(theta), 0, Mathf.Cos(theta), 0));
        m_out.SetRow(3, new Vector4(0, 0, 0, 1));

        return m_out;
    }

    static Matrix4x4 rotz(float theta, bool is_rad = true)
    {
        if (!is_rad)
            theta *= Mathf.Deg2Rad;
      //       | cos(angle) - sin(angle) 0 |
      //   T = | sin(angle)  cos(angle) 0 |
      //       | 0           0      1 |
        var m_out = new Matrix4x4();
        m_out.SetRow(0, new Vector4(Mathf.Cos(theta), -1 * Mathf.Sin(theta), 0, 0));
        m_out.SetRow(1, new Vector4(Mathf.Sin(theta), Mathf.Cos(theta), 0, 0));
        m_out.SetRow(2, new Vector4(0, 0, 1, 0));
        m_out.SetRow(3, new Vector4(0, 0, 0, 1));

        return m_out;
    }

    public static Vector3 quat2Eul(Quaternion q)
    {
        Vector3 angles = new Vector3();

        // roll (x-axis rotation)
        float sinr_cosp = 2 * (q.w * q.x + q.y * q.z);
        float cosr_cosp = 1 - 2 * (q.x * q.x + q.y * q.y);
        angles.x = Mathf.Atan2(sinr_cosp, cosr_cosp);

        // pitch (y-axis rotation)
        float sinp = 2 * (q.w * q.y - q.z * q.x);
        if (Mathf.Abs(sinp) >= 1)
        {
            // angles.y = std::copysign(Mathf.PI / 2, sinp); // use 90 degrees if out of range
            if (sinp < 0)
                angles.y = -1 * Mathf.PI / 2;
            else
                angles.y = Mathf.PI / 2;
        }
        else
            angles.y = Mathf.Asin(sinp);

        // yaw (z-axis rotation)
        float siny_cosp = 2 * (q.w * q.z + q.x * q.y);
        float cosy_cosp = 1 - 2 * (q.y * q.y + q.z * q.z);
        angles.z = Mathf.Atan2(siny_cosp, cosy_cosp);

        return angles;        
    }

    public static Vector2 rot2d(Vector2 v, float theta)
    {
        // cos(th) sin(th)
        // -sin(th) cos(th)

        return new Vector2((v.x * Mathf.Cos(theta) + v.y * Mathf.Sin(theta)), (-1 * v.x * Mathf.Sin(theta) + v.y * Mathf.Cos(theta)));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
