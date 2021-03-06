﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TesseractDemo
{
    public partial class Form1 : Form
    {

        private Point pLeftUpper;
        private Point pRightDown;
        private Rectangle rect_selected;
        bool isMouseDown = false;

        DataTable dtLabels;

        bool IsZoomIn = false;
        private Point pLeftUpper_Croped;
        Image image_ori = null;

        public Form1()
        {
            InitializeComponent();

            //define table
            dtLabels = new DataTable();
            dtLabels.Columns.Add("LABEL_NAME", typeof(string));
            dtLabels.Columns.Add("X1", typeof(int));
            dtLabels.Columns.Add("Y1", typeof(int));
            dtLabels.Columns.Add("X2", typeof(int));
            dtLabels.Columns.Add("Y2", typeof(int));
            dtLabels.Columns.Add("IMAGE", typeof(Image));//FRO PREVIEW
            dtLabels.Columns.Add("VAL", typeof(string));

            gvLabels.AutoGenerateColumns = false;
            gvLabels.DataSource = dtLabels; // set data source
        }

        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            string sFileName = "";

            try
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                //filter
                fileDialog.Filter = "圖片|*.bmp;*.jpg;*.png";
                //show folder dialog
                if (fileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                if (image_ori != null)
                {
                    image_ori.Dispose();
                }

                sFileName = fileDialog.FileName;

                //Image img = Image.FromFile(sFileName);
                image_ori = Image.FromFile(sFileName);
                //Bitmap bitmap = new Bitmap(img);
                pictureBox1.Image = image_ori;

                //img.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            
            if (pictureBox1.Image == null) { return; }
            Refresh();
            pLeftUpper = new Point();
            pRightDown = new Point();
            isMouseDown = true;
            pLeftUpper = e.Location;

            ReportLoaction(e.Location); //回報座標
        }


        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                pRightDown = e.Location;
                //ReportLoaction(e.Location); //回報座標
                ReportAllLoaction(pLeftUpper, pRightDown); //左上、右下
                isMouseDown = false;
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (pictureBox1.Image == null) { return; }
            if (isMouseDown == true)
            {
                pRightDown = e.Location;
                ReportLoaction(e.Location); //回報座標
                Refresh();
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (rect_selected != null)
            {
                rect_selected = new Rectangle();
                rect_selected.X = Math.Min(pLeftUpper.X, pRightDown.X);
                rect_selected.Y = Math.Min(pLeftUpper.Y, pRightDown.Y);
                rect_selected.Width = Math.Abs(pLeftUpper.X - pRightDown.X);
                rect_selected.Height = Math.Abs(pLeftUpper.Y - pRightDown.Y);
                e.Graphics.DrawRectangle(Pens.Red, rect_selected);
            }
        }

        private void ReportLoaction(Point location1)
        {
            richTextBox1.Text = $"X={location1.X}\r\nY={location1.Y}";
        }

        private void ReportAllLoaction(Point location1,Point location2)
        {
            richTextBox1.Text = $"X1={location1.X}\r\nY1={location1.Y}\r\n";
            richTextBox1.Text += $"X2={location2.X}\r\nY2={location2.Y}";

            //取得真實座標
            Point realLocation1 = GetActualLocation(location1);
            Point realLocation2 = GetActualLocation(location2);
            richTextBox1.Text += "\r\n Real Location：\r\n";
            richTextBox1.Text += $"X1={realLocation1.X}\r\nY1={realLocation1.Y}\r\n";
            richTextBox1.Text += $"X2={realLocation2.X}\r\nY2={realLocation2.Y}";
        }

        private Point GetActualLocation(Point location1)
        {
            Int32 mouseX = location1.X;
            Int32 mouseY = location1.Y;
            //圖片實際大小
            Int32 realW = pictureBox1.Image.Width;
            Int32 realH = pictureBox1.Image.Height;
            //目前畫面上的大小
            Int32 currentW = pictureBox1.ClientRectangle.Width;
            Int32 currentH = pictureBox1.ClientRectangle.Height;
            //計算縮放比例
            Double zoomW = (currentW / (Double)realW);
            Double zoomH = (currentH / (Double)realH);
            Double zoomActual = Math.Min(zoomW, zoomH);
            //依縮放換算真實座標
            Double padX = zoomActual == zoomW ? 0 : (currentW - (zoomActual * realW)) / 2;
            Double padY = zoomActual == zoomH ? 0 : (currentH - (zoomActual * realH)) / 2;
            Int32 realX = (Int32)((mouseX - padX) / zoomActual);
            Int32 realY = (Int32)((mouseY - padY) / zoomActual);
            int x = realX < 0 || realX > realW ? -1 : realX;
            int y = realY < 0 || realY > realH ? -1 : realY;
            return new Point(x, y);
        }

        private void ttbLabel_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if(e.KeyCode != Keys.Enter) { return; }

                string sLabelName = ttbLabel.Text.Trim();

                if(sLabelName == "")
                {
                    throw new Exception("LabelName不可為空!!");
                }

                Point location1 = GetActualLocation(pLeftUpper);
                Point location2 = GetActualLocation(pRightDown);

                //如果Zoom In下框選，需要補上相對位置
                if (IsZoomIn)
                {
                    location1.X += pLeftUpper_Croped.X;
                    location1.Y += pLeftUpper_Croped.Y;
                    location2.X += pLeftUpper_Croped.X;
                    location2.Y += pLeftUpper_Croped.Y;
                }

                DataRow dr_new = dtLabels.NewRow();
                dr_new["LABEL_NAME"] = sLabelName;
                dr_new["X1"] = location1.X;
                dr_new["Y1"] = location1.Y;
                dr_new["X2"] = location2.X;
                dr_new["Y2"] = location2.Y;

                dtLabels.Rows.Add(dr_new);

                ttbLabel.Text = "";
                ttbLabel.Focus();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void btnGetTaggedImage_Click(object sender, EventArgs e)
        {
            try
            {
                if(dtLabels.Rows.Count == 0)
                {
                    return; //
                }

                await ImagePreProcessAsync();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async Task ImagePreProcessAsync()
        {
            Task[] tasks = new Task[dtLabels.Rows.Count];
            int iIndex = 0;

            string sImgPath = @".\img\";
            ImageHelper ih = new ImageHelper();
            //string sLabelName = "";

            foreach (DataRow dr_label in dtLabels.Rows)
            {
                Bitmap source = new Bitmap(pictureBox1.Image);
                
                //每一個Task處理Label (非同步方式執行)
                tasks[iIndex] = Task.Run(() => {

                    string sLabelName = dr_label["LABEL_NAME"].ToString();
                    Point p1 = new Point((int)dr_label["X1"], (int)dr_label["Y1"]);
                    Point p2 = new Point((int)dr_label["X2"], (int)dr_label["Y2"]);

                    //抓取標記的範圍
                    Bitmap img = ih.Crop(source, p1, p2);
                    img.Save($"{sImgPath}{sLabelName}-Step1-Ori.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                    //放大五倍
                    img = ih.Resize(img, img.Width * 5, img.Height * 5);
                    img.Save($"{sImgPath}{sLabelName}-Step2-Resize.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                    //轉成灰階
                    img = ih.SetGrayscale(img);
                    img.Save($"{sImgPath}{sLabelName}-Step3-SetGrayscale.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                    //反轉(檢查黑大於白的話，進行反轉)
                    img = ih.Invert(img);
                    img.Save($"{sImgPath}{sLabelName}-Step4-Invert.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                    //高斯模糊(為了解決文字解析度或字型造成缺口問題)
                    img = ih.GaussianBlur(img);
                    img.Save($"{sImgPath}{sLabelName}-Step5-GaussianBlur.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                    //轉成絕對黑白
                    img = ih.SetToBW(img, 190);
                    img.Save($"{sImgPath}{sLabelName}-Step6-ConvertTo1Bpp1.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                    //更新Image欄位
                    dr_label.BeginEdit();
                    dr_label["IMAGE"] = img;
                    dr_label["VAL"] = OCRHelper.OCR(img); //傳入圖片進行OCR
                    dr_label.EndEdit();
                }
                );
                iIndex++;
            }

            Task.WaitAll(tasks); // 等待所有Task完成後
        }

        /// <summary>
        /// 進入圖片區時，開啟放大功能(Timer Start)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            if(pictureBox1.Image == null) { return; }
            timer1.Interval = 1 * 100;
            timer1.Start();
            
        }

        /// <summary>
        /// 離開圖片區時，取消放大功能(Timer Stop)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            timer1.Stop();
            pictureBox2.Image = null;
        }

        /// <summary>
        /// 每0.1秒，顯示mouse目前區域且放大2倍示顯
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                int x = Control.MousePosition.X;
                int y = Control.MousePosition.Y;

                int magnification = 2;//倍率
                int imgWidth = pictureBox2.Width;
                int imgHeight = pictureBox2.Height;

                Bitmap bt = new Bitmap(imgWidth / magnification, imgHeight / magnification);
                using(Graphics g = Graphics.FromImage(bt))
                {
                    g.CopyFromScreen(
                         new Point(Cursor.Position.X - imgWidth / (2 * magnification),
                                   Cursor.Position.Y - imgHeight / (2 * magnification)),
                         new Point(0,0),
                         new Size(imgWidth / magnification, imgHeight / magnification));
                    IntPtr dc1 = g.GetHdc();
                    g.ReleaseHdc(dc1);
                }

                pictureBox2.Image = (Image)bt;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            try
            {
                if(IsZoomIn)
                {
                    UnCrop(); 
                }
                else
                {
                    if (pLeftUpper == Point.Empty || pRightDown == Point.Empty)
                    {
                        throw new Exception("末選取縮放範圍!!");
                    }

                    Crop();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Crop()
        {
            Point pLeftUpper_ACT = GetActualLocation(pLeftUpper);
            Point pRightDown_ACT = GetActualLocation(pRightDown);
            //get croped location
            pLeftUpper_Croped = pLeftUpper_ACT;

            pLeftUpper = Point.Empty;
            pRightDown = Point.Empty;

            ImageHelper ih = new ImageHelper();
            Image img_crop = ih.Crop((Bitmap)image_ori, pLeftUpper_ACT, pRightDown_ACT);

            pictureBox1.Image = img_crop;
            IsZoomIn = true;
            btnZoomIn.Text = "Zoom Out";
        }

        private void UnCrop()
        {
            //uncrop
            btnZoomIn.Text = "Zoom In";
            IsZoomIn = false;
            pLeftUpper_Croped = Point.Empty;
            pLeftUpper = Point.Empty;
            pRightDown = Point.Empty;

            if (image_ori != pictureBox1.Image)
            {
                pictureBox1.Image = image_ori;
            }
            Refresh();
        }
    }
}
