using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;

namespace BXHSerialPort
{
    public partial class frmMain : Form
    {
        private SerialPort ComDevice = new SerialPort();

        public frmMain()
        {
            InitializeComponent();
            init();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <remarks>Created At Time: [ 2017-10-23 20:45 ], By User:lishuai, On Machine:Brian-NB</remarks>
        public void init()
        {

            lbl_status.Text = "设备连线中，请稍后...";
            pictureBox1.BackgroundImage = Properties.Resources.red;
            OpenCom();
            ComDevice.DataReceived += new SerialDataReceivedEventHandler(Com_DataReceived);//绑定事件

        }

        /// <summary>
        /// Opens the COM.
        /// </summary>
        /// <remarks>Created At Time: [ 2017-10-23 20:46 ], By User:lishuai, On Machine:Brian-NB</remarks>
        private void OpenCom() 
        {
            if (ComDevice.IsOpen == false)
            {
                ComDevice.PortName = "COM1";
                ComDevice.BaudRate = 9600;
                ComDevice.Parity = Parity.None;
                ComDevice.DataBits = 8;
                ComDevice.StopBits = StopBits.One;
                try
                {
                    ComDevice.Open();
                    lbl_status.Text = "已连线";

                    pictureBox1.BackgroundImage = Properties.Resources.green;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
               
               
            }
            else
            {
                try
                {
                    ComDevice.Close();
                   
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                lbl_status.Text = "已关闭连线";
                pictureBox1.BackgroundImage = Properties.Resources.red;
            }
        
        }


        /// <summary>
        /// 关闭串口
        /// </summary>
        public void ClearSelf()
        {
            if (ComDevice.IsOpen)
            {
                ComDevice.Close();
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            lbl_weight.Text = "0.0";
            this.WindowState = FormWindowState.Maximized;

            #region regedit
            #region 注册
            //RegistryKey RootKey, RegKey;

            ////项名为：HKEY_CURRENT_USER\Software
            //RootKey = Registry.CurrentUser.OpenSubKey("Software", true);

            ////打开子项：HKEY_CURRENT_USER\Software\MyRegDataApp1
            //if ((RegKey = RootKey.OpenSubKey("MyRegDataApp1", true)) == null)
            //{
            //    RootKey.CreateSubKey("MyRegDataApp1");//不存在，则创建子项
            //    RegKey = RootKey.OpenSubKey("MyRegDataApp1", true);
            //    RegKey.SetValue("UseTime", (object)19);  //创建键值，存储可使用次数
            //    MessageBox.Show("您可以免费使用本软件20次！", "感谢您首次使用");
            //    return;
            //}

            //try
            //{
            //    object usetime = RegKey.GetValue("UseTime");//读取键值，可使用次数
            //    MessageBox.Show("你还可以使用本软件 :" + usetime.ToString() + "次！", "确认", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    int newtime = Int32.Parse(usetime.ToString()) - 1;

            //    if (newtime < 0)
            //    {
            //        if (MessageBox.Show("继续使用，请购买本软件！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
            //        {
            //            // Application.Exit();
            //            System.Environment.Exit(0);
            //        }
            //    }
            //    else
            //    {
            //        RegKey.SetValue("UseTime", (object)newtime);//更新键值，可使用次数减1
            //    }
            //}
            //catch
            //{
            //    RegKey.SetValue("UseTime", (object)10); //创建键值，存储可使用次数
            //    MessageBox.Show("您可以免费使用本软件10次！", "感谢您首次使用");
            //    return;
            //}
            #endregion


            #endregion

        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Com_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] ReDatas = new byte[ComDevice.BytesToRead];
            ComDevice.Read(ReDatas, 0, ReDatas.Length);//读取数据

            this.AddData(ReDatas);//输出数据
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="data">字节数组</param>
        public void AddData(byte[] data)
        {
                AddContent(new ASCIIEncoding().GetString(data));
      
        }

        /// <summary>
        /// 输入到显示区域
        /// </summary>
        /// <param name="content"></param>
        private void AddContent(string content)
        {
            this.BeginInvoke(new MethodInvoker(delegate
            {
               
                #region edit by brian
                txtShowData.AppendText(content);
                if (this.txtShowData.Text.Contains("kg"))
                {
                    string[] sarry = txtShowData.Text.Split(new string[] { "kg" }, StringSplitOptions.RemoveEmptyEntries);
                    if (sarry.Length > 0)
                    {
                        string tempdata = sarry[0].Trim().ToString();
                        //string tempdata2 = tempdata.Substring(tempdata.Length - 7, 7).Trim().ToString();
                        AddConByBrian(tempdata);
                        this.txtShowData.Clear();
                    }
                }
              

                #endregion


              
            }));
        }


        private void AddConByBrian(string a)
        {
            this.BeginInvoke(new MethodInvoker(delegate
            {
                lbl_weight.Text = "";
                if (a.Length > 0)
                {
                    lbl_weight.Text = a;
                    // call 打印标签
                    PrintLabel();

                    if (ConfirmYesNo("确认本托已经称重打印完毕，并继续称重下一托？"))
                    {
                        // call 保存数据到CSV
                        SaveData();
                        DeleteRow();
                    }

                    
                }

            }));
        }


        /// <summary>
        /// Prints the label.
        /// </summary>
        /// <param name="vendor">The vendor.</param>
        /// <param name="date">The date.</param>
        /// <param name="pn">The pn.</param>
        /// <param name="descrition">The descrition.</param>
        /// <param name="po">The po.</param>
        /// <param name="qty">The qty.</param>
        /// <param name="sn">The sn.</param>
        /// <param name="k1">The k1.</param>
        /// <param name="k2">The k2.</param>
        /// <param name="end">The end.</param>
        /// <remarks>Created At Time: [ 2017-10-30 22:36 ], By User:lishuai, On Machine:Brian-NB</remarks>
        private void PrintLabel() 
        {

            string vendor=string.Empty;
            string date=string.Empty;
            string pn=string.Empty;
            string descrition=string.Empty;
            string po=string.Empty;
            string qty=string.Empty;
            string sn=string.Empty;
            string k1=string.Empty;
            string k2=string.Empty;
            string end = string.Empty;
            string ph = string.Empty;//生产批号;
          

            try
            {
                
                if (dataGridView1.Rows.Count > 0)
                {

                    vendor = dataGridView1.CurrentRow.Cells[0].Value.ToString();
                    DateTime dt = DateTime.Now;
                    string sdt = dt.GetDateTimeFormats('g')[0].ToString();  //供应商
                    date = sdt;
                    pn = dataGridView1.CurrentRow.Cells[5].Value.ToString();//料号
                    ph = dataGridView1.CurrentRow.Cells[2].Value.ToString();//批号
                    descrition = dataGridView1.CurrentRow.Cells[6].Value.ToString();//描述
                    po = dataGridView1.CurrentRow.Cells[1].Value.ToString();//订单

                    qty = dataGridView1.CurrentRow.Cells[9].Value.ToString();//总托数

                    sn = dataGridView1.CurrentRow.Cells[4].Value.ToString();//托号
                    k1 = dataGridView1.CurrentRow.Cells[7].Value.ToString();//原始重量
                    k2 = lbl_weight.Text;                                   //实际重量
                    


                   // double cy = Convert.ToDouble(dataGridView1.CurrentRow.Cells[7].Value) - Convert.ToDouble(lbl_weight.Text);
                    double cy =Convert.ToDouble(lbl_weight.Text)- Convert.ToDouble(dataGridView1.CurrentRow.Cells[7].Value) ;
                   
                    cy = System.Math.Abs(cy);

                    if (cy <= 1)
                    {
                        end = "OK";
                    }
                    else
                    {
                        end = "NG";
                    }


                    #region print
                    LabelManager2.Application labelapp = new LabelManager2.Application(); //创建lppa.exe进程
                    string strPath = System.Windows.Forms.Application.StartupPath + "\\102X152.Lab";


                    labelapp.Documents.Open(strPath, false);
                    LabelManager2.Document labeldoc = labelapp.ActiveDocument;

                    labeldoc.Variables.FormVariables.Item("VENDOR").Value = vendor;
                    labeldoc.Variables.FormVariables.Item("DATE").Value = date;
                    labeldoc.Variables.FormVariables.Item("PN").Value = pn;
                    labeldoc.Variables.FormVariables.Item("PH").Value = ph;//批号
                    labeldoc.Variables.FormVariables.Item("DSP").Value = descrition;
                    labeldoc.Variables.FormVariables.Item("PO").Value = po;
                    labeldoc.Variables.FormVariables.Item("QTY").Value = qty;
                    labeldoc.Variables.FormVariables.Item("SN").Value = sn;
                    labeldoc.Variables.FormVariables.Item("K1").Value = k1;
                    labeldoc.Variables.FormVariables.Item("K2").Value = k2;
                    labeldoc.Variables.FormVariables.Item("END").Value = end;




                    labeldoc.PrintDocument(); //打印一次
                    labeldoc.FormFeed(); //结束打印

                    labeldoc.Close(true);
                    labelapp.Application.Quit();

                    #endregion

                }
                else
                {
                    MessageBox.Show("请先选中要称重的托盘数据！！");
                    return;
                }



            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }



            
        }

        /// <summary>
        /// 显示一个YesNo选择对话框
        /// </summary>
        /// <param name="prompt">对话框的选择内容提示信息</param>
        /// <returns>如果选择Yes则返回true，否则返回false</returns>
        public static bool ConfirmYesNo(string prompt)
        {
            return MessageBox.Show(prompt, "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        /// <summary>
        /// Saves the data to csv
        /// </summary>
        /// <remarks>Created At Time: [ 2017-10-23 22:39 ], By User:lishuai, On Machine:Brian-NB</remarks>
        private void SaveData() 
        {
            try
            {
                if (dataGridView1.Rows.Count > 0)
                {
                    StreamWriter sw;
                    string data = string.Empty;
                    string longdate = System.DateTime.Now.ToString("D");
                    if (!File.Exists(longdate+".csv"))
                    {
                        sw = new StreamWriter(longdate + ".csv", true, Encoding.GetEncoding("gb2312"));
                        data = "供应商名称,订单号码,生产批次号,送货时间,托盘号,料号,描述,原始重量,实际重量,差异重量,是否合格";
                        sw.WriteLine(data);
                        sw.Close();
                    }

                    sw = new StreamWriter(longdate + ".csv", true, Encoding.GetEncoding("gb2312"));
                    DateTime dt = DateTime.Now;
                    string sdt=dt.GetDateTimeFormats('g')[0].ToString();
                    string str = string.Empty;

                  
               
                    data = dataGridView1.CurrentRow.Cells[0].Value + "," + dataGridView1.CurrentRow.Cells[1].Value + "," + dataGridView1.CurrentRow.Cells[2].Value + ",";
                    data += sdt + "," + dataGridView1.CurrentRow.Cells[4].Value + "," + dataGridView1.CurrentRow.Cells[5].Value + ",";
                    data += dataGridView1.CurrentRow.Cells[6].Value + "," + dataGridView1.CurrentRow.Cells[7].Value + "," + lbl_weight.Text + ",";


                    //double cy = Convert.ToDouble(dataGridView1.CurrentRow.Cells[7].Value) - Convert.ToDouble(lbl_weight.Text);
                    double cy = Convert.ToDouble(lbl_weight.Text)-Convert.ToDouble(dataGridView1.CurrentRow.Cells[7].Value) ;
                    string IsOk = "";
                    double cy2 = System.Math.Abs(cy);
                   
                    if (cy2 <= 1)
                    {
                        IsOk = "OK";
                    }
                    else
                    {
                        IsOk = "NG";
                    }
                    data += cy + ",";
                    data += IsOk;
                    //OutputEncoding = Encoding.Unicode;
                    sw.WriteLine(data);

                    sw.Close();

                   // UpdateExcel(this.textBox1.Text, "Sheet1", dataGridView1.CurrentRow.Cells[1].Value.ToString(), dataGridView1.CurrentRow.Cells[2].Value.ToString(), Convert.ToInt32(dataGridView1.CurrentRow.Cells[4].Value));

                    //button2_Click(null, null);

                }
                else
                {
                    MessageBox.Show("请先选中要称重的托盘数据！！");
                    return;
                }

              
               
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        
        }

        /// <summary>
        /// Updates the excel.更新状态
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="tobeOpenSheet">The tobe open sheet.</param>
        /// <param name="orderno">The orderno.</param>
        /// <param name="pihao">The pihao.</param>
        /// <param name="tuopanhao">The tuopanhao.</param>
        /// <remarks>Created At Time: [ 2017-10-23 23:47 ], By User:lishuai, On Machine:Brian-NB</remarks>
        public void UpdateExcel(string filePath, string tobeOpenSheet,string orderno,string pihao,int tuopanhao)
        {
            // string strConn = getExcelOleDBConnectStr(filePath);
            string strConn = "Provider=Microsoft.ACE.OLEDB.12.0;"
             + "Data Source= " + this.textBox1.Text + ";" + "Extended Properties='Excel 12.0; HDR=Yes; IMEX=4'";
           
            OleDbConnection conn = new OleDbConnection(strConn);

            try
            {
                conn.Open();
                string strExcel = "";
                OleDbCommand cmd = null;
                //strExcel = string.Format("update [" + tobeOpenSheet + "$] set 状态=1 where 订单号码={0} and 生产批次号={1} and 托盘号={2} and 状态=0 ",Convert.ToInt32(orderno),Convert.ToInt32(pihao),tuopanhao);
                strExcel = string.Format("update [" + tobeOpenSheet + "$] set 状态=1 where 托盘号='{0}' and 状态=0 ", tuopanhao.ToString());
      
                cmd = new OleDbCommand(strExcel, conn);
                int i = cmd.ExecuteNonQuery();
                if (i>0)
                {
                   // MessageBox.Show("称重成功，请进行下一笔称重！");
                }
                else
                {
                    MessageBox.Show("称重数据保存失败，请联系管理员处理！");
                }

            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
            }

            
        }

        /// <summary>
        /// 选择供应商Excel数据
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <remarks>Created At Time: [ 2017-10-23 21:07 ], By User:lishuai, On Machine:Brian-NB</remarks>
        private void button1_Click(object sender, EventArgs e)
        {
           // openFileDialog1.InitialDirectory = "d:\\";//注意这里写路径时要用c:\\而不是c:\
            //openFileDialog1.Filter = "xls files (*.xls,*.xlsx)|*.xls,*.xlsx";
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.FilterIndex = 1;


            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.textBox1.Text = this.openFileDialog1.FileName;
            }
            else
            {
                this.textBox1.Text = "";
            }
        }

        /// <summary>
        /// 导入供应商Excel数据
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <remarks>Created At Time: [ 2017-10-23 21:07 ], By User:lishuai, On Machine:Brian-NB</remarks>
        private void button2_Click(object sender, EventArgs e)
        {
            string fileName = "";
            fileName = this.textBox1.Text;
            if (this.textBox1.Text != "")
            {
                try
                {
                    DataSet ds = new DataSet();

                    ds = this.excelToDataSet(this.textBox1.Text, "Sheet1");


                    if (ds.Tables[0].Columns.Count > 0)
                    {
                        this.dataGridView1.DataSource = ds.Tables[0];
                        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;//列宽自适应
                        SetGridColor();
                    }
                    else
                    {
                        this.dataGridView1.DataSource = null;
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("请选择案件导入的EXCEL");

                }



            }
            else
            {
                MessageBox.Show("请选择Excel文件");
            }
        }

        void SetGridColor() 
        {
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.GreenYellow;
            //行Header的背景色为橙色
            dataGridView1.RowHeadersDefaultCellStyle.BackColor = Color.Lime;
        }

        private void BindGrid1()
        {
            //string sfilename = AppDomain.CurrentDomain.BaseDirectory;
            string sfilename = this.textBox1.Text;
            //string sfilename = AppDomain.CurrentDomain.BaseDirectory + "\\DVR保证书数据.csv";
            string cnstring = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + sfilename + ";Extended Properties=\"text;HDR=YES;FMT=Delimited\";";
            //string cnstring = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=c:\\txtFilesFolder\\;Extended Properties=\"text;HDR=Yes;FMT=Delimited\";";
            //HDR=Yes表示有列头，No表示无列头
            OleDbConnection cn = new OleDbConnection(cnstring);
            string aSQL = "select * from DVR保证书数据.csv ";
            cn.Open();
            OleDbDataAdapter da = new OleDbDataAdapter(aSQL, cn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            cn.Close();
            dataGridView1.DataSource = dt;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.GreenYellow;
            //行Header的背景色为橙色
            dataGridView1.RowHeadersDefaultCellStyle.BackColor = Color.Lime;




        }



        /// <summary>
        /// excel to datagrid
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="tobeOpenSheet">The tobe open sheet.</param>
        /// <returns></returns>
        /// <remarks>Created At Time: [ 2017-10-23 21:07 ], By User:lishuai, On Machine:Brian-NB</remarks>
        public DataSet excelToDataSet(string filePath, string tobeOpenSheet)
        {
            // string strConn = getExcelOleDBConnectStr(filePath);
            string strConn = "Provider=Microsoft.ACE.OLEDB.12.0;"
             + "Data Source= " + this.textBox1.Text + ";" + "Extended Properties='Excel 12.0; HDR=Yes; IMEX=1'";
            DataSet ds = null;
            OleDbConnection conn = new OleDbConnection(strConn);

            try
            {
                conn.Open();
                string strExcel = "";
                OleDbDataAdapter myCommand = null;
                strExcel = "select * from [" + tobeOpenSheet + "$] where 状态=0 ";
                myCommand = new OleDbDataAdapter(strExcel, strConn);
                ds = new DataSet();

                myCommand.Fill(ds);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
            }

            return ds;
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex <= 0)
            {
                //dataGridView1.Rows[e.RowIndex].HeaderCell.
                dataGridView1.Rows[e.RowIndex].HeaderCell.Value = (e.RowIndex + 1).ToString();
                 dataGridView1.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.lbl_weight.Text = "828";

            //print
            PrintLabel();
            if (ConfirmYesNo("确认本托已经称重打印完毕，并继续称重下一托？"))
            {
                // call 保存数据到CSV
                SaveData();
                DeleteRow();
            }
          
        }

        void DeleteRow() 
        {
            if (null == dataGridView1.CurrentCell)
            {
                return;
            }
            int RowNumber = dataGridView1.CurrentCell.RowIndex;
            dataGridView1.Rows.RemoveAt(RowNumber);
            this.dataGridView1.Refresh();
        
        }


    }
}
