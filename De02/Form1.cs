using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using De02.Models;

namespace De02
{
    public partial class frmSanpham : Form
    {
        private Sanpham _selectedProduct;
        private bool isProductModified = false;
        public frmSanpham()
        {
            InitializeComponent();
        }

        private void frmSanpham_Load(object sender, EventArgs e)
        {
            this.FormClosing += frmSanpham_FormClosing;
            try
            {
                using (SanPhamDB context = new SanPhamDB())
                {
                    List<LoaiSP> listTypes = context.LoaiSP.ToList();
                    List<Sanpham> listProducts = context.Sanpham.ToList();
                    FillComboBox(listTypes);
                    BindGrid(listProducts);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }
        private void FillComboBox(List<LoaiSP> listTypes)
        {
            cboLoaiSP.DataSource = listTypes;
            cboLoaiSP.DisplayMember = "TenLoai";
            cboLoaiSP.ValueMember = "MaLoai";
        }

        private void BindGrid(List<Sanpham> listProducts)
        {
            dgvSanpham.Rows.Clear();
            foreach (var item in listProducts)
            {
                int index = dgvSanpham.Rows.Add();
                dgvSanpham.Rows[index].Cells[0].Value = item.MaSP;
                dgvSanpham.Rows[index].Cells[1].Value = item.TenSP;
                dgvSanpham.Rows[index].Cells[2].Value = item.NgayNhap;
                dgvSanpham.Rows[index].Cells[3].Value = item.MaLoai?.ToString();
            }
        }

        private void btThem_Click(object sender, EventArgs e)
        {
            try
            {
                using (SanPhamDB context = new SanPhamDB())
                {
                    if (string.IsNullOrWhiteSpace(txtMaSP.Text) || string.IsNullOrWhiteSpace(txtTenSP.Text))
                    {
                        MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo");
                        return;
                    }
                    string maLoai = cboLoaiSP.SelectedValue?.ToString();
                    if (string.IsNullOrEmpty(maLoai))
                    {
                        MessageBox.Show("Vui lòng chọn loại sản phẩm!", "Thông báo");
                        return;
                    }
                    var existingProduct = context.Sanpham.Find(txtMaSP.Text);
                    if (existingProduct != null)
                    {
                        MessageBox.Show("Mã sản phẩm đã tồn tại!", "Cảnh báo");
                        return;
                    }
                    var newProduct = new Sanpham
                    {
                        MaSP = txtMaSP.Text,
                        TenSP = txtTenSP.Text,
                        NgayNhap = dtNgaynhap.Value,
                        MaLoai = maLoai
                    };
                    context.Sanpham.Add(newProduct);
                    context.SaveChanges();
                    BindGrid(context.Sanpham.ToList());
                    MessageBox.Show("Thêm sản phẩm thành công!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void dgvSanpham_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvSanpham.Rows[e.RowIndex];
                string maSP = row.Cells[0].Value.ToString();

                using (SanPhamDB context = new SanPhamDB())
                {
                    _selectedProduct = context.Sanpham.FirstOrDefault(sp => sp.MaSP == maSP);

                    if (_selectedProduct != null)
                    {
                        txtMaSP.Text = _selectedProduct.MaSP;
                        txtTenSP.Text = _selectedProduct.TenSP;
                        dtNgaynhap.Value = (DateTime)_selectedProduct.NgayNhap;
                        cboLoaiSP.SelectedValue = _selectedProduct.MaLoai;
                    }
                    else
                    {
                        MessageBox.Show("Sản phẩm không tồn tại trong cơ sở dữ liệu!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btThoat_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btSua_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvSanpham.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn sản phẩm cần sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                string maSP = dgvSanpham.SelectedRows[0].Cells[0].Value.ToString();

                using (SanPhamDB context = new SanPhamDB())
                {
                    var productToUpdate = context.Sanpham.FirstOrDefault(sp => sp.MaSP == maSP);
                    if (productToUpdate != null)
                    {
                        bool isModified = false;
                        if (productToUpdate.TenSP != txtTenSP.Text)
                        {
                            productToUpdate.TenSP = txtTenSP.Text;
                            isModified = true;
                        }

                        if (productToUpdate.NgayNhap != dtNgaynhap.Value)
                        {
                            productToUpdate.NgayNhap = dtNgaynhap.Value;
                            isModified = true;
                        }

                        if (productToUpdate.MaLoai != cboLoaiSP.SelectedValue.ToString())
                        {
                            productToUpdate.MaLoai = cboLoaiSP.SelectedValue.ToString();
                            isModified = true;
                        }
                        if (isModified)
                        {
                            context.SaveChanges();
                            MessageBox.Show("Cập nhật sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            BindGrid(context.Sanpham.ToList());
                        }
                        else
                        {
                            MessageBox.Show("Không có thay đổi nào để sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy sản phẩm cần sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi sửa dữ liệu: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btXoa_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvSanpham.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn sản phẩm cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string maSP = dgvSanpham.SelectedRows[0].Cells[0].Value.ToString();

                using (SanPhamDB context = new SanPhamDB())
                {
                    var productToDelete = context.Sanpham.FirstOrDefault(sp => sp.MaSP == maSP);

                    if (productToDelete != null)
                    {
                        context.Sanpham.Remove(productToDelete);
                        context.SaveChanges();
                        MessageBox.Show("Xóa sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        BindGrid(context.Sanpham.ToList());
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy sản phẩm cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa dữ liệu: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btLuu_Click(object sender, EventArgs e)
        {
            try
            {
                if (!isProductModified)
                {
                    MessageBox.Show("Không có thay đổi nào để lưu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string maSP = txtMaSP.Text;

                using (SanPhamDB context = new SanPhamDB())
                {
                    var productToSave = context.Sanpham.FirstOrDefault(sp => sp.MaSP == maSP);

                    if (productToSave != null)
                    {
                        productToSave.TenSP = txtTenSP.Text;
                        productToSave.NgayNhap = dtNgaynhap.Value;
                        productToSave.MaLoai = cboLoaiSP.SelectedValue.ToString();

                        context.SaveChanges();

                        MessageBox.Show("Lưu sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        isProductModified = false;

                        BindGrid(context.Sanpham.ToList());
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy sản phẩm để lưu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu dữ liệu: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btKLuu_Click(object sender, EventArgs e)
        {
            try
            {
                if (!isProductModified)
                {
                    MessageBox.Show("Không có thay đổi nào để hủy.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                DataGridViewRow row = dgvSanpham.SelectedRows[0];
                txtMaSP.Text = row.Cells[0].Value.ToString();
                txtTenSP.Text = row.Cells[1].Value.ToString();
                dtNgaynhap.Value = (DateTime)row.Cells[2].Value;
                cboLoaiSP.SelectedValue = row.Cells[3].Value.ToString();

                isProductModified = false;

                MessageBox.Show("Đã hủy thay đổi.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi hủy thay đổi: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void frmSanpham_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (isProductModified)
                {
                    var result = MessageBox.Show("Bạn có muốn lưu các thay đổi?", "Thông báo", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        btLuu_Click(sender, e);
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi kiểm tra đóng form: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnTim_Click(object sender, EventArgs e)
        {
            string searchTerm = txtFind.Text.Trim();

            if (string.IsNullOrEmpty(searchTerm))
            {
                MessageBox.Show("Vui lòng nhập từ khóa tìm kiếm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SanPhamDB context = new SanPhamDB())
                {
                    var searchResults = context.Sanpham
                                               .Where(sp => sp.MaSP.Contains(searchTerm) || sp.TenSP.Contains(searchTerm))
                                               .ToList();

                    if (searchResults.Any())
                    {
                        BindGrid(searchResults);
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy sản phẩm nào.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void frmSanpham_FormClosing_1(object sender, FormClosingEventArgs e)
        {
            {
                DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn đóng không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
