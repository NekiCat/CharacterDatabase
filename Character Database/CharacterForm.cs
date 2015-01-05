﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Character_Database
{
    public partial class CharacterForm : Form
    {
        public Character Character { get; set; }

        public CharacterForm()
        {
            InitializeComponent();
        }

        private void CharacterForm_Shown(object sender, EventArgs e)
        {
            Text = Character.Name;
            textBox1.Text = Character.Name;
            textBox2.Text = Character.Description;
            textBox3.Text = Character.Tags;
            if (File.Exists(Character.Picture)) pictureBox1.Image = Image.FromFile(Character.Picture);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Character.Picture = openFileDialog1.FileName;
                pictureBox1.Image = Image.FromFile(Character.Picture);
            }
        }

        private void CharacterForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Character.Name = textBox1.Text;
            Character.Description = textBox2.Text;
            Character.Tags = textBox3.Text;
        }
    }
}