using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace FLib
{
    public class Skeleton
    {
        public readonly Joint Root;
        public readonly List<Joint> Joints;
        public int Count { get { return Joints.Count; } }

        public Skeleton(Joint root)
        {
            Root = root;
            Joints = new List<Joint>();
            if (root != null)
            {
                SerializeJoints(root);
            }
        }
        /// スケルトンからの関節リスト生成
        private void SerializeJoints(Joint joint)
        {
            Joints.Add(joint);
            foreach (Joint child in joint.Children)
            {
                SerializeJoints(child);
            }
        }
        /// グローバルポーズの計算
        public void UpdateGlobalPose()
        {
            Root.UpdateGlobalPose();
        }

        //----------------------------------------------------------------------

        /// スケルトンの描画
        public void Draw(GraphicsDevice GraphicsDevice)
        {
            // 全ての子ノードを再帰的に描画
            foreach (Joint child in Root.Children)
            {
                DrawSkeletonNode(GraphicsDevice, child);
            }
        }

        /// モデルの描画
        protected void DrawModel(GraphicsDevice GraphicsDevice, Model model, Matrix world, Vector3 diffuseColor)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = world;
                    effect.DiffuseColor = diffuseColor;
                }
                mesh.Draw();
            }
        }

        void DrawJoint(GraphicsDevice GraphicsDevice, Joint joint)
        {
            if (joint == null || joint.Parent == null)
            {
                return;
            }
            //            DrawModel(GraphicsDevice, Matrix.CreateScale(3) * joint.Parent.GlobalPose, new Vector3(1, 0, 0));
            foreach (Joint child in joint.Children)
            {
                DrawJoint(GraphicsDevice, child);
            }
        }

        /// スケルトンを再帰的に辿りながらの全てのボーンを描画
        protected void DrawSkeletonNode(GraphicsDevice GraphicsDevice, Joint joint)
        {
            if (joint == null || joint.Parent == null)
            {
                return;
            }
            //            DrawModel(GraphicsDevice, unitSphere, joint.InvBindPose * joint.Parent.GlobalPose, new Vector3(0.8f, 0.8f, 0.8f));
            foreach (Joint child in joint.Children)
            {
                DrawSkeletonNode(GraphicsDevice, child);
            }
        }
    }
}