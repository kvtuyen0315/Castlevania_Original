# -*- coding: utf-8 -*-
"""
run_all.py - chạy toàn bộ quy trình kiểm kê, ghi lại MỌI file đầu ra trong thư mục inventory\\ (cha của tools\\).

Cách chạy (Git Bash / PowerShell) TỪ GỐC REPO, KHÔNG dùng -I:
    python -P -E Tools/cvgp/run_all.py
Đầu ra: ClaudeNotes/cvgp/inventory/ (đổi bằng biến môi trường CVGP_INV_OUT). Chạy từ thư mục nào cũng được (đường dẫn tính từ vị trí script).

Thứ tự: inv_sprites.py (4 sheet nhân vật/enemy/boss/item) -> inv_level1.py (bản đồ Level 1) -> make_inventory_md.py (SHEETS_INVENTORY.md).
Đầu vào là 5 file PNG trong Art_Source/spriters-resource/ (chỉ đọc); không đụng tới dự án Unity.
Chạy lại cho kết quả y hệt (không dùng số ngẫu nhiên, không ghi thời gian vào file).
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import inv_sprites   # noqa: E402
import inv_level1    # noqa: E402
import make_inventory_md   # noqa: E402


def main():
    inv_sprites.main(["inv_sprites.py"])
    inv_level1.main()
    make_inventory_md.main()


if __name__ == "__main__":
    main()
