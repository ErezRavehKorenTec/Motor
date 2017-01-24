using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motor.Enums
{
    public enum DataErrorEnum
    {
        Tried_to_SET_Unknown_Variable_Or_Flag = 20,
        Tried_to_SET_to_an_incorrect_value,
        VI_set_greater_than_or_equal_to_VM,
        VM_set_less_than_or_equal_to_VI,
        Illegal_Data_Entered,
        Variable_or_Flag_is_Read_Only,
        Variable_or_Flag_not_allowed_to_be_Incremented_or_Decremented,
        Trip_Not_Defined,
        Trying_to_Redefine_a_Program_Label_or_GLOBAL_User_Variable,
        Trying_to_Redefine_an_Embedded_Command_or_Variable,
        Unknown_Label_or_User_Variable,
        Program_Label_Or_User_Variable_Table_is_Full,
        Trying_to_SET_a_Label,
        Error_Codes_Lexium_MCo, de_Reference_Manual,
        Trying_to_SET_an_Instruction,
        Trying_to_Exec_a_Variable_or_Flag,
        Trying_to_Print_Illegal_variable_or_flag,
        Illegal_Motor_Count_to_Encoder_Count_Ratio,
        Command_Or_Variable_OrFlag_Not_Available_in_Drive,
        Missing_parameter_separator,
        Trip_on_Position_and_Trip_on_Relative_distance_not_allowed_together,

    }
}


