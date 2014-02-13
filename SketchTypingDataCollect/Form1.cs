using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using FLib;


namespace SketchTypingDataCollect
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            gestureForm = new GestureForm(this);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string text = "NewCommand" + treeView1.Nodes.Count;
            treeView1.Nodes.Add(text);
            commands.Add(new SketchTypeCommand(text));
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node != null)
            {
                currentComId =  e.Node.Index;
                UpdateCommandView();
            }
        }

        void UpdateCommandView()
        {
            if (currentComId < 0 || commands.Count <= currentComId) return;
            var com = commands[currentComId];
            inputText.Text = com.Text;
            imageList1.Images.Clear();
            listView1.Clear();
            foreach (var kv in com.gestureImages)
            {
                imageList1.Images.Add(kv.Key, kv.Value);
                listView1.Items.Add(kv.Key, kv.Key);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (0 <= currentComId && currentComId < treeView1.Nodes.Count)
            {
                treeView1.Nodes[currentComId].Text = inputText.Text;
                commands[currentComId].Text = inputText.Text;
            }
        }

        GestureForm gestureForm;
        int currentComId = -1;
        List<SketchTypeCommand> commands = new List<SketchTypeCommand>();

        private void button2_Click(object sender, EventArgs e)
        {
            gestureForm.HooAndShow();
            if (0 <= currentComId && currentComId < commands.Count)
            {
                if (gestureForm.stroke != null && gestureForm.stroke.Count >= 1)
                {
                    if (!commands[currentComId].gestureImages.ContainsKey(gestureForm.inputText))
                    {
                        commands[currentComId].AddNewGesture(
                            gestureForm.inputText, new List<List<Point>>() { gestureForm.stroke }, 
                            imageList1.ImageSize.Width, imageList1.ImageSize.Height);
                        imageList1.Images.Add(gestureForm.inputText, commands[currentComId].gestureImages[gestureForm.inputText]);
                        listView1.Items.Add(gestureForm.inputText, gestureForm.inputText);
                    }
                }
            }
        }

        string gesturePath = "";

        private void saveSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (System.IO.File.Exists(gesturePath))
            {
                SketchTypeCommand.SaveCommands(gesturePath, commands);
            }
            else
            {
                saveAsToolStripMenuItem_Click(sender, e);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.Filter = "*.txt|*.txt";
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                gesturePath = saveFileDialog1.FileName;
                SketchTypeCommand.SaveCommands(gesturePath, commands);
            }
        }

        private void loadLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Filter = "*.txt|*.txt";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                gesturePath = openFileDialog1.FileName;
                commands = SketchTypeCommand.LoadCommands(gesturePath, imageList1.ImageSize.Width, imageList1.ImageSize.Height);
                treeView1.Nodes.Clear();
                foreach (var com in commands)
                {
                    treeView1.Nodes.Add(com.Text);
                }
            }
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (listView1.SelectedItems != null && listView1.SelectedItems.Count >= 1 && e.KeyCode == Keys.Delete)
            {
                for (int i = 0; i < listView1.SelectedItems.Count; i++)
                {
                    commands[currentComId].RemoveGesture(listView1.SelectedItems[i].Text);
                    listView1.Items.Remove(listView1.SelectedItems[i]);
                }
            }
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (treeView1.SelectedNode != null && e.KeyCode == Keys.Delete && 0 <= currentComId && currentComId < commands.Count)
            {
                int idx = treeView1.SelectedNode.Index;
                commands.RemoveAt(currentComId);
                treeView1.Nodes.Remove(treeView1.SelectedNode);
                if (currentComId >= idx)
                {
                    currentComId--;
                }
                UpdateCommandView();
            }
        }
    }
}
