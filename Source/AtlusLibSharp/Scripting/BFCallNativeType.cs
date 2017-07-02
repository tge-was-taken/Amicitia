
public enum BfCallNativeType
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
	MsgDisp = 0, // takes 1 argument, the message handle to display
	
	/// <summary>
	///	Takes 1 argument eg: "PUT(@sel)"
	/// 1. The value to put in the variable in the stack.
	///	</summary>
	Put = 2, // from test.scr

	/// <summary>
	///	Takes 2 arguments eg: "MSG(LABEL02, 1);" where LABEL02 is 3
	/// Argument 1: The message dialogue id to display
	///	</summary>
	Msg = 4, // from test.scr, takes 2 arguments

    /// <summary>
    /// Takes in 1 argument.
    /// Argument 1: Flag to get.
    /// </summary>
    FlagGet = 7,

    /// <summary>
    /// Takes in 1 argument.
    /// Argument 1: Flag to set.
    /// </summary>
    FlagSet = 8,

    /// <summary>
    /// Takes in 1 argument.
    /// Argument 1. Flag to clear.
    /// </summary>
    FlagClear = 9,
	
	/// <summary>
	///	Takes 1 argument eg: "MSG_WND_CLS(1)"
	///	</summary>
	MsgWndCls = 29, // from test.scr
	
	/// <summary>
	///	Takes 3 arguments eg: "MSG_WND_DSP(1, 3, 0)"
	///	</summary>
	MsgWndDsp = 30, // from test.scr
	
	/// <summary>
	///	Takes 3 arguments eg: "MSG_SEL(TEST_SEL, 0, 0);"
	///	</summary>
	MsgSel = 31, // from test.scr

    /// <summary>
    ///	Takes 1 argument eg: "ITF_YESNO_DSP(0)"
    ///	</summary>
    ItfYesnoDsp = 61, // from test.scr
	
	/// <summary>
	///	Takes no arguments.
	///	</summary>
	ItfYesnoCls = 62, // from test.scr
	
	// test.scr: "@sel = ITF_YESNO_ANS();"
	/// <summary>
	///	Takes no arguments.
	///	</summary>
	ItfYesnoAns = 63, // from test.scr
	
	/// <summary>
	///	Takes 3 arguments eg: "ITF_SEL(0, TEST, TEST_SEL);" 
	///	</summary>
	ItfSel = 362, // from test.scr
	
	/// <summary>
	///	Takes 2 arguments eg: "SCR_SET_COMVAR(0, 1)"
	///	</summary>
	ScrSetComvar = 366, // from test.scr
	
	/// <summary>
	///	Takes 1 argument eg: "SCR_INC_COMVAR(0)"
	///	</summary>
	ScrIncComvar = 367, // from test.scr
	
	/// <summary>
	///	Takes 1 argument eg: "SCR_DEC_COMVAR(0)"
	///	</summary>
	ScrDecComvar = 368, // from test.scr

    // test.scr: "PUT(SCR_GET_COMVAR(0));"
    /// <summary>
    /// Takes 1 argument eg: "SCR_GET_COMVAR(0)"
    ///	</summary>
    ScrGetComvar = 369, // from test.scr
	
	/// <summary>
	///	Takes 3 arguments eg: "RUMBLE_START(30, 30, 2)"
	///	</summary>
	RumbleStart = 374, // from test.scr
	
	/// <summary>
	///	Takes no arguments.
	///	</summary> 
	RumbleStop = 375, // from test.scr
}