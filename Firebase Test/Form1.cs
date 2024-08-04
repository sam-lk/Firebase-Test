using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;

namespace Firebase_Test
{
    public partial class Form1 : Form
    {
        DataTable dt = new DataTable();

        //======== CONFIG FIREBASE SETTIGNs
        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "WFxhKTYq4m5JCvXiCCAGG2l1rpTTfjUFBLTDF35NGFDFGDFS",
            BasePath = "https://fir-test-5996a.firebaseioss.com/"
        };

        IFirebaseClient myClient;

        public Form1()
        {
            InitializeComponent();

        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            //==== CONNECT FIREBASE AS CLIENT
            myClient = new FireSharp.FirebaseClient(config);

            //==== CHECK FIREBASE CONNECTION STATUS
            if (myClient != null)
            {
                panelConStatus.BackColor = Color.LimeGreen;
                lblConStatus.Text = "Connected";
            }
            else
            {
                panelConStatus.BackColor = Color.IndianRed;
                lblConStatus.Text = "Disconnected";
                return;
            }

            //====== READ COUNTER COLLECTION (prev cus_id)
            FirebaseResponse respo = await myClient.GetTaskAsync("CusDataCounter/CurrentRead");
            CounterData CounterD1 = respo.ResultAs<CounterData>();

            textBox1.Text = (Convert.ToInt32(CounterD1.totcount) + 1).ToString(); //increment prev cus_id and set

            //====== ADD COLUMNS : DATAGRIDVIEW
            dt.Columns.Add("ID");
            dt.Columns.Add("Name");
            dt.Columns.Add("Town");
            dt.Columns.Add("Age");
            dataGridView1.DataSource = dt;

        }

        //INSERT NEW CUS DATA
        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //====== READ FIREBASE COUNTER COLLECTION (prev cus_id)
                FirebaseResponse respo = await myClient.GetTaskAsync("CusDataCounter/CurrentRead");
                CounterData CounterD1 = respo.ResultAs<CounterData>();
                string lastReadRow = CounterD1.currntid;

                //====== INSERT DATA : FIREBASE
                var data = new Data
                {
                    Id = (Convert.ToInt32(CounterD1.totcount) + 1).ToString(),
                    Name = textBox2.Text,
                    Town = textBox3.Text,
                    Age = textBox4.Text
                };
                SetResponse myResponse = await myClient.SetTaskAsync("CustomerData/" + data.Id, data);
                Data myResult = myResponse.ResultAs<Data>();

                MessageBox.Show("Successfully Added.. " + myResult.Id);

                //====== UPDATE FIREBASE COUNTER COLLECTION (current inserted cus_id)
                var Obj = new CounterData
                {
                    
                    totcount = data.Id,
                    currntid = lastReadRow
                };
                SetResponse updateCouter = await myClient.SetTaskAsync("CusDataCounter/CurrentRead", Obj);

                //INCREMENT ID AND CLREAR FIELDS
                textBox1.Text = (Convert.ToInt32(data.Id) + 1).ToString();
                textBox2.Text = "";
                textBox3.Text = "";
                textBox4.Text = "";
            }
            catch(Exception ex)
            {
                MessageBox.Show("Data Insert Error Catch : " + ex.Message);
            }
        }

        //RETRIVE SELECTED CUS_ID DATA
        private async void button2_Click(object sender, EventArgs e)
        {
            try
            {
                FirebaseResponse response = await myClient.GetTaskAsync("CustomerData/" + textBox1.Text);
                string JsTxt = response.Body;
                if (JsTxt == "null")
                {
                    MessageBox.Show("Incorrect ID");
                    return;
                }
                Data myDataObj = response.ResultAs<Data>();

                textBox1.Text = myDataObj.Id;
                textBox2.Text = myDataObj.Name;
                textBox3.Text = myDataObj.Town;
                textBox4.Text = myDataObj.Age;

                MessageBox.Show("Data Retrived Successfully");
            }
            catch(Exception ex)
            {
                MessageBox.Show("Data Retrive Error Catch : " + ex.Message);
            }
        }

        //READ ALL DATA
        private async void button3_Click(object sender, EventArgs e)
        {
            //==== READ COUNTER COLLECTION DATA
            FirebaseResponse respo1 = await myClient.GetTaskAsync("CusDataCounter/CurrentRead");
            CounterData ReceivedData1 = respo1.ResultAs<CounterData>();
            int lastRowCount = Convert.ToInt32(ReceivedData1.totcount);//total row count
            int i = Convert.ToInt32(ReceivedData1.currntid); //last readline
            Console.WriteLine("Total Row Count : "+lastRowCount + " | Last ReadRow : "+ i);

            while(true)
            {
                //==== CHECK TOTAL COUNT AND PREV LAST READLINE
                if(lastRowCount == i)
                {
                    break;
                }

                i++;
                try
                {
                    //== READ ALL CUSTOMER DATA (i)
                    FirebaseResponse respo2 = await myClient.GetTaskAsync("CustomerData/" + i);
                    string JsTxt = respo2.Body;
                    if (JsTxt == "null")
                    {
                        MessageBox.Show("No data");
                        return;
                    }
                    Data ReceivedData2 = respo2.ResultAs<Data>();

                    //ADD TO DATAGRIDVIEW TABLE
                    DataRow MyRow = dt.NewRow();
                    MyRow["ID"] = ReceivedData2.Id;
                    MyRow["Name"] = ReceivedData2.Name;
                    MyRow["Town"] = ReceivedData2.Town;
                    MyRow["Age"] = ReceivedData2.Age;
                    dt.Rows.Add(MyRow);

                    //UPDATE 
                    var Obj = new CounterData
                    {
                        totcount = lastRowCount.ToString(),
                        currntid = i.ToString()
                    };
                    SetResponse updateCouter = await myClient.SetTaskAsync("CusDataCounter/CurrentRead", Obj);

                }
                catch(Exception ex)
                {
                    MessageBox.Show("Data Retrive All Error Catch : " + ex.Message);
                }
            }

            MessageBox.Show("Done..!!");
        }
    }
}
