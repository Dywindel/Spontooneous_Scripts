using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RL_Clock: MonoBehaviour {

	// This script is the in-game clock.
	// It will be accessed by other game scripts that require time

	private GM gM;

	// Syncing clock test
	public bool syncClock = false;

	// Reading from the beginning of 2018, don't know what problems this may cause
	DateTime epochStart;
	private int refYear = 2018;

	[System.NonSerialized]
	private double startTime;
	// Realworld time as doubles
	public double rW_Sec_D;
	// Current Time as doubles
	[HideInInspector]
	public double cT_Mil_D, cT_Sec_D, cT_Min_D, cT_Hr_D, cT_Day_D, cT_Mon_D, cT_Ssn_D, cT_Yr_D;

	[HideInInspector]
	// Current time as floats
	public float cT_Sec_F, cT_Min_F, cT_Hr_F, cT_Day_F, cT_Mon_F, cT_Ssn_F, cT_Yr_F;
	// Current time as floats, but normalised to 1
	[HideInInspector]
	public float cT_Sec_F_Norm, cT_Min_F_Norm, cT_Hr_F_Norm, cT_Day_F_Norm, cT_Mon_F_Norm, cT_Ssn_F_Norm, cT_Yr_F_Norm;

	// Current time as integers
	public int cT_Sec_I, cT_Min_I, cT_Hr_I, cT_Day_I, cT_Mon_I, cT_Ssn_I, cT_Yr_I;

	public double timeSpeed = 1.0f;	// Allows me to speed up and slow down time

	// Conversion factors
	double convFact_IRLSecsToInGameSecs = 0.3;			// Conversion from RL time to IG time 1s = 1a -> I can change this whenever
	double convFact_MiliToSecs = 1000; 		// Converts the curTime from miliseconds to seconds
	[System.NonSerialized]
	public double convFact_Secs = 9;		// Number of seconds in a minute etc...
	[System.NonSerialized]
	public double convFact_Mins = 9;
	[System.NonSerialized]
	public double convFact_Hours = 9;
	[System.NonSerialized]
	public double convFact_Days = 27;
	[System.NonSerialized]
	public double convFact_Months = 3;
	[System.NonSerialized]
	public double convFact_Seasons = 4;

	// Daytime lengths
	public float dayLength_Day = 0.6f;
	[HideInInspector]
	private float dayLength_Day_Max = 0.7f;
	[HideInInspector]
	private float dayLength_Day_Min = 0.3f;
	[HideInInspector]
	public float dayLength_Transition = 0.04f;
	[HideInInspector]
	public bool bool_isNightTime = true;

	// Sun Settings
	[System.NonSerialized]
	public float sunHeight_Max = 89.0f;
	[System.NonSerialized]
	public float sunHeight_Min = 89.0f;

	// Moon Settings
	[System.NonSerialized]
	public double moonOrbit_Days = 3.03d;
	[System.NonSerialized]
	public double moonTilt_Hours = 27.33d;

	// We only want to update the clock values once per second (To save computing time)
	float updateTime = 0f;
	[Range (0.01f, 1f)]
	public float updatePeriod = 0.01f;

	void Start()
	{
		// Reference to world variables
		if (GameObject.FindGameObjectWithTag("GM") != null)
		{
			GameObject gM_GameObject = GameObject.FindGameObjectWithTag("GM");
			gM = gM_GameObject.GetComponent<GM>();
		}
		else
			gM = null;

		// Noting when I take into account epoch start
		epochStart = new DateTime(refYear, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		// This is the time that is grabbed when the player first opens the game, reading from their internal clock
		cT_Sec_D = (System.DateTime.UtcNow - epochStart).TotalMilliseconds/(convFact_IRLSecsToInGameSecs*convFact_MiliToSecs);
		rW_Sec_D = cT_Sec_D;
	}

	void FixedUpdate()
	{
		// The current time In-Game as a double in seconds, correct for framerate
		cT_Sec_D += convFact_IRLSecsToInGameSecs*Time.deltaTime*timeSpeed;
		// The current time in the real world as a double, in seconds
		rW_Sec_D += Time.deltaTime*timeSpeed;

		// I don't need to perform the rest of these calculations once per frame, that's too much
		// I can perform them once per second
		if (updateTime > 0f)
		{
			updateTime -= (float)(Time.deltaTime);
		}
		else
		{
			// Reset the updateTime
			updateTime = (float)updatePeriod;

			cT_Min_D = cT_Sec_D/convFact_Secs;
			cT_Hr_D = cT_Min_D/convFact_Mins;
			cT_Day_D = cT_Hr_D/convFact_Hours;
			cT_Mon_D = cT_Day_D/convFact_Days;
			cT_Ssn_D = cT_Mon_D/convFact_Months;
			cT_Yr_D = cT_Ssn_D/convFact_Seasons;
			
			// Current time In-Game as floats
			cT_Sec_F = (float)(cT_Sec_D % convFact_Secs);
			cT_Min_F = (float)(cT_Min_D % convFact_Mins);
			cT_Hr_F = (float)(cT_Hr_D % convFact_Hours);
			cT_Day_F = (float)(cT_Day_D % convFact_Days);
			cT_Mon_F = (float)(cT_Mon_D % convFact_Months);
			cT_Ssn_F = (float)(cT_Mon_D % convFact_Seasons);
			cT_Yr_F = (float)(cT_Yr_D % 1d);

			// Current time In-Game as floats, but normalised to 1
			cT_Hr_F_Norm = cT_Hr_F/(float)convFact_Hours;

			// Current time In-Game as ints
			cT_Sec_I = (int)(cT_Sec_F);
			cT_Min_I = (int)(cT_Min_F);
			cT_Hr_I = (int)(cT_Hr_F);
			cT_Day_I = (int)(cT_Day_F);
			cT_Mon_I = (int)(cT_Mon_F);
			cT_Ssn_I = (int)(cT_Ssn_F);
			cT_Yr_I = (int)(cT_Yr_F);

			// Some actions require a nighttime or daytime boolean
			// This boolean changes at the beginning of the day/night transition
			if (gM != null)
			{
				if ((cT_Hr_F_Norm >= dayLength_Day) && (cT_Hr_F_Norm < (1 - dayLength_Transition)))
				{
					bool_isNightTime = true;
				}
				else
				{
					bool_isNightTime = false;
				}
			}

			// Day length is based on year time
			dayLength_Day = ((dayLength_Day_Max - dayLength_Day_Min)/2.0f)*Mathf.Sin(cT_Yr_F*2*Mathf.PI) + ((dayLength_Day_Max - dayLength_Day_Min)/2.0f + dayLength_Day_Min);

			// Sun height is based on its position during the year
			sunHeight_Max = ((89.0f - 60.0f)/2.0f)*Mathf.Sin(cT_Yr_F*2*Mathf.PI) + (89.0f - (((89.0f - 60.0f)/2.0f)));
			sunHeight_Min = ((50.0f - 25.0f)/2.0f)*Mathf.Sin(cT_Yr_F*2*Mathf.PI) + (50.0f - (((50.0f - 25.0f)/2.0f)));
		}
	}
}
