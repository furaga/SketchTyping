using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FLib
{
    /// 関節クラス
    public class Joint
    {
        public string Name { get; set; }
        public Joint Parent { get; private set; }
        public List<Joint> Children;
        public Vector3 Scales;
        public Quaternion Rotation;
        public Vector3 Translation;
        public Matrix LocalPose { get { return Matrix.CreateScale(Scales) * Matrix.CreateFromQuaternion(Rotation) * Matrix.CreateTranslation(Translation); } }
        public Matrix GlobalPose { get; set; }
        public Matrix InvBindPose;
        public Joint(string name, Vector3 translation)
        {
            Parent = null;
            Children = new List<Joint>();
            Translation = translation;
            Scales = Vector3.One; ;
            Rotation = Quaternion.Identity;
            CalcInvBindPose(translation);
            Name = Name;
        }
        public Joint(Joint joint)
        {
            Scales = joint.Scales;
            Parent = joint.Parent;
            Children = joint.Children;
            Translation = joint.Translation;
            Rotation = joint.Rotation;
            GlobalPose = joint.GlobalPose;
            InvBindPose = joint.InvBindPose;
            Name = joint.Name;
        }
        public void AddChild(Joint child)
        {
            Children.Add(child);
            child.Parent = this;
        }
        public void UpdateGlobalPose()
        {
            CalcInvBindPose(Translation);
            GlobalPose = LocalPose;
            if (Parent != null)
            {
                GlobalPose *= Parent.GlobalPose;
            }
            foreach (Joint child in Children)
            {
                child.UpdateGlobalPose();
            }
        }
        public void UpdateLocalPoseFromGlobalPose(Vector3 parentTrans)
        {
            var trans = GlobalPose.Translation;
            Translation = GlobalPose.Translation - parentTrans;
            foreach (Joint child in Children)
            {
                child.UpdateLocalPoseFromGlobalPose(trans);
            }
        }

        public void CalcInvBindPose(Vector3 translation)
        {
            if (translation.Length() <= 0.0f)
            {
                InvBindPose = Matrix.Identity;
            }
            else
            {
                Vector3 vz = Vector3.Zero;
                vz.Z = 1.0f;

                float fa = (float)Math.Acos((double)translation.Z / translation.Length());
                Vector3 vaxis = Vector3.Cross(vz, translation);
                vaxis = Vector3.Normalize(vaxis);
                if (fa == 0 || fa == MathHelper.Pi)
                {
                    vaxis = Vector3.Normalize(translation);
                    fa = 0;
                }
                Matrix invBindPose;
                Matrix.CreateScale(ref Scales, out invBindPose);
                invBindPose *= Matrix.CreateTranslation(0.0f, 0.0f, translation.Length() / 2.0f);
                invBindPose *= Matrix.CreateFromAxisAngle(vaxis, fa);
                InvBindPose = invBindPose;
            }
        }
    }
}