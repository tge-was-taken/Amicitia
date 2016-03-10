
public enum BFCallNativeType
{
	// Commented out native calls in test.scr
	// BUSTUP(0, 1, 1);
	// BUSTUP_CLS();
	// COMIC_EVENT_PAUSE(0);
	// COMIC_EVENT_WAIT(20);
	
	/// <summary>
	///	Takes 1 argument:
	/// 1. The message handle to display.
	///	</summary>
	MSG_DISP = 0, // takes 1 argument, the message handle to display
	
	/// <summary>
	///	Takes 1 argument eg: "PUT(@sel)"
	/// 1. The value to put in the variable in the stack.
	///	</summary>
	PUT = 2, // from test.scr

	/// <summary>
	///	Takes 2 arguments eg: "MSG(LABEL02, 1);" where LABEL02 is 3
	/// Argument 1: The message dialogue id to display
	///	</summary>
	MSG = 4, // from test.scr, takes 2 arguments

    /// <summary>
    /// Takes in 1 argument.
    /// Argument 1: Flag to get.
    /// </summary>
    FLAG_GET = 7,

    /// <summary>
    /// Takes in 1 argument.
    /// Argument 1: Flag to set.
    /// </summary>
    FLAG_SET = 8,

    /// <summary>
    /// Takes in 1 argument.
    /// Argument 1. Flag to clear.
    /// </summary>
    FLAG_CLEAR = 9,
	
	/// <summary>
	///	Takes 1 argument eg: "MSG_WND_CLS(1)"
	///	</summary>
	MSG_WND_CLS = 29, // from test.scr
	
	/// <summary>
	///	Takes 3 arguments eg: "MSG_WND_DSP(1, 3, 0)"
	///	</summary>
	MSG_WND_DSP = 30, // from test.scr
	
	/// <summary>
	///	Takes 3 arguments eg: "MSG_SEL(TEST_SEL, 0, 0);"
	///	</summary>
	MSG_SEL = 31, // from test.scr

    /// <summary>
    ///	Takes 1 argument eg: "ITF_YESNO_DSP(0)"
    ///	</summary>
    ITF_YESNO_DSP = 61, // from test.scr
	
	/// <summary>
	///	Takes no arguments.
	///	</summary>
	ITF_YESNO_CLS = 62, // from test.scr
	
	// test.scr: "@sel = ITF_YESNO_ANS();"
	/// <summary>
	///	Takes no arguments.
	///	</summary>
	ITF_YESNO_ANS = 63, // from test.scr
	
	/// <summary>
	///	Takes 3 arguments eg: "ITF_SEL(0, TEST, TEST_SEL);" 
	///	</summary>
	ITF_SEL = 362, // from test.scr
	
	/// <summary>
	///	Takes 2 arguments eg: "SCR_SET_COMVAR(0, 1)"
	///	</summary>
	SCR_SET_COMVAR = 366, // from test.scr
	
	/// <summary>
	///	Takes 1 argument eg: "SCR_INC_COMVAR(0)"
	///	</summary>
	SCR_INC_COMVAR = 367, // from test.scr
	
	/// <summary>
	///	Takes 1 argument eg: "SCR_DEC_COMVAR(0)"
	///	</summary>
	SCR_DEC_COMVAR = 368, // from test.scr

    // test.scr: "PUT(SCR_GET_COMVAR(0));"
    /// <summary>
    /// Takes 1 argument eg: "SCR_GET_COMVAR(0)"
    ///	</summary>
    SCR_GET_COMVAR = 369, // from test.scr
	
	/// <summary>
	///	Takes 3 arguments eg: "RUMBLE_START(30, 30, 2)"
	///	</summary>
	RUMBLE_START = 374, // from test.scr
	
	/// <summary>
	///	Takes no arguments.
	///	</summary> 
	RUMBLE_STOP = 375, // from test.scr
}