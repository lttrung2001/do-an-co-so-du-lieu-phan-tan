﻿using DevExpress.Data;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using GridView = DevExpress.XtraGrid.Views.Grid.GridView;

namespace QLDSV_TC
{
    public partial class frmDangKyMon : Form
    {
        public frmDangKyMon()
        {
            InitializeComponent();
        }

        private void frmDangKyMon_Load(object sender, EventArgs e)
        {
            this.taNienKhoa.Connection.ConnectionString = Program.connectionString;
            this.taNienKhoa.Fill(this.dS.NIENKHOA);

            cmbNienKhoa.DataSource = bdsNienKhoa;
            cmbNienKhoa.ValueMember = "NIENKHOA";
            cmbNienKhoa.DisplayMember = "NIENKHOA";

            cmbHocKy.SelectedIndex = 0;
        }

        private void btnXacNhan_Click(object sender, EventArgs e)
        {
            if (teMaSV.Text.Equals(""))
            {
                MessageBox.Show("Không để trống mã sinh viên!");
                return;
            }
            Program.myReader.Close();
            Program.myReader = Program.ExecSqlDataReader(String.Format("EXEC SP_LAYTHONGTINSINHVIEN '{0}'", teMaSV.Text), 
                                                            Program.connectionString);
            if (Program.myReader.HasRows)
            {
                Program.myReader.Read();
                teHoTen.Text = Program.myReader["HOTEN"].ToString();
                teMaLop.Text = Program.myReader["MALOP"].ToString();

                pnlThongTinSV.Enabled = false;
                pnlLocLopTinChi.Enabled = true;
            }
            else
            {
                MessageBox.Show("Mã sinh viên không tồn tại!");
            }
        }

        private void btnLoc_Click(object sender, EventArgs e)
        {
            try
            {
                sP_LOCLTCTableAdapter.Connection.ConnectionString = Program.connectionString;
                sP_LOCLTCTableAdapter.Fill(dS.SP_LOCLTC, teMaSV.Text, cmbNienKhoa.Text, int.Parse(cmbHocKy.Text));

                sP_LAY_DSLTC_SVDKTableAdapter.Connection.ConnectionString = Program.connectionString;
                sP_LAY_DSLTC_SVDKTableAdapter.Fill(dS.SP_LAY_DSLTC_SVDK, teMaSV.Text, cmbNienKhoa.Text, int.Parse(cmbHocKy.Text));

                sP_LOCLTCGridControl.Enabled = 
                    sP_LAY_DSLTC_SVDKGridControl.Enabled = 
                    groupBox1.Enabled =
                    panel1.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi khi lọc lớp tín chỉ: " + ex.Message);
            }
        }

        private void btnThoatDangKy_Click(object sender, EventArgs e)
        {
            pnlThongTinSV.Enabled = true;
            pnlLocLopTinChi.Enabled = 
                sP_LOCLTCGridControl.Enabled = 
                sP_LAY_DSLTC_SVDKGridControl.Enabled = 
                groupBox1.Enabled =
                panel1.Enabled = false;
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            // Lưu dữ liệu trước khi thoát form
            Close();
        }

        private void gridView1_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (sP_LOCLTCBindingSource.Current == null) return;
            String huy = ((DataRowView)sP_LOCLTCBindingSource.Current)["HUYDANGKY"].ToString();
            if (huy.Equals("False"))
                button1.Text = "Hủy đăng ký";
            else
                button1.Text = "Đăng ký";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DataRowView currentRow = (DataRowView)sP_LOCLTCBindingSource.Current;
            String huy = currentRow["HUYDANGKY"].ToString();
            if (huy.Equals("False")) // Ban đầu đã đăng ký
            {
                currentRow["HUYDANGKY"] = true; // Set lại trạng thái là Hủy
                button1.Text = "Đăng ký";
                sP_LAY_DSLTC_SVDKBindingSource.RemoveAt(kiemTraTrungMon(currentRow["MAMH"].ToString()));
            }
            else // Ban đầu chưa đăng ký hoặc đang hủy
            {
                // Check đã đăng ký môn này?
                if (kiemTraTrungMon(currentRow["MAMH"].ToString()) != -1)
                    MessageBox.Show("Bạn đã đăng ký môn này rồi!");
                else // Không trùng
                {
                    currentRow["HUYDANGKY"] = false;
                    button1.Text = "Hủy đăng ký";
                    sP_LAY_DSLTC_SVDKBindingSource.AddNew();
                    DataRowView newRow = ((DataRowView)sP_LAY_DSLTC_SVDKBindingSource.Current);
                    newRow["MALTC"] = currentRow["MALTC"];
                    newRow["MAMH"] = currentRow["MAMH"];
                    newRow["TENMH"] = currentRow["TENMH"];
                    newRow["NHOM"] = currentRow["NHOM"];
                    newRow["HOTENGV"] = currentRow["HOTENGV"];
                }
            }
        }
        private int kiemTraTrungMon(String maMon)
        {
            DataRowView row;
            for (int i = 0; i < sP_LAY_DSLTC_SVDKBindingSource.Count; i++)
            {
                row = (DataRowView)sP_LAY_DSLTC_SVDKBindingSource[i];
                if (maMon.Equals(row["MAMH"].ToString()))
                    return i; // Trung thi tra ve vi tri
            }
            return -1; // Khong trung
        }

        private void cmbHocKy_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("MALTC", typeof(int));

            DataRowView tmp;
            for (int i = 0; i < sP_LAY_DSLTC_SVDKBindingSource.Count; i++)
            {
                tmp = ((DataRowView)sP_LAY_DSLTC_SVDKBindingSource[i]);
                dt.Rows.Add(int.Parse(tmp["MALTC"].ToString()));
            }

            SqlParameter param = new SqlParameter();
            param.SqlDbType = SqlDbType.Structured;
            param.TypeName = "dbo.TYPE_CAPNHATDANGKY";
            param.ParameterName = "@DK";
            param.Value = dt;

            SqlParameter param2 = new SqlParameter();
            param2.SqlDbType = SqlDbType.NChar;
            param2.ParameterName = "@MASV";
            param2.Value = teMaSV.Text;

            Program.KetNoi();

            SqlCommand cmd = new SqlCommand("SP_CAPNHATDANGKY", Program.conn);
            cmd.Parameters.Clear();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(param);
            cmd.Parameters.Add(param2);
            try
            {
                cmd.ExecuteNonQuery();
                MessageBox.Show("Ghi thành công!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ghi thất bại: " + ex.Message);
            }
        }
    }
}
