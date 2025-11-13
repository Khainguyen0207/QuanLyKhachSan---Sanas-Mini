using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyKhachSan.Enums
{
    public enum CustomerStatusEnum
    {
        [Description("Active")]
        active = 0,

        [Description("Locked")]
        locked = 1,
    }

    
}
