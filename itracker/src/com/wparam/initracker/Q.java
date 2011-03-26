package com.wparam.initracker;

import android.content.Context;
import android.widget.Toast;
import android.util.Log;

public class Q
{
	public static void test1 ()
	{
		System.out.printf ("blah %i\n");
	}

	public static void toastprint (Context c, String s)
	{
		Context context = c.getApplicationContext();
		CharSequence text = s;

		Toast toast = Toast.makeText(context, text, Toast.LENGTH_SHORT);
		toast.show();
	}

	private static String doformat (String f, Object... args)
	{
		String out;

		try
		{
			out = String.format (f, args);
		}
		catch (Exception w)
		{
			out = "FORMAT EXCEPTION";
		}

		return out;
	}


	public static void toastprintf (Context c, String f, Object... args)
	{
		toastprint (c, doformat (f, args));
	}

    public static String getTag (Context c)
    {
        if (c != null)
			return c.getString (R.string.debug_tag);
		else
			return "BAD CONTEXT";
    }

	public static void logprintf (Context c, int level, 
					String f, Object... args)
    {
        logprintf (getTag (c), level, f, args);
    }

	public static void logprintf (String tag, int level, 
					String f, Object... args)
	{
		Log.println (level, tag, doformat (f, args));
	}

    public static void d (String tag, String f, Object... args)
    {
        Log.println (Log.DEBUG, tag, doformat (f, args));
    }

    public static void d (Context c, String f, Object... args)
    {
        d (getTag (c), f, args);
    }

	public static void log_d_e (Context c, Throwable th)
    {
        log_d_e (getTag (c), th);
    }

	public static void log_d_e (String tag, Throwable th)
	{
		Log.d (tag, "exception:", th);
	}


	private Context thecontext;
	private Q() {}
	public Q(Context c) { thecontext = c ; }

	public void tprint (String s)
	{
		toastprint (thecontext, s);
	}

	public void tprintf (String f, Object... args)
	{
		toastprintf (thecontext, f, args);
	}

	public void tp (String f, Object... args)
	{
		toastprintf (thecontext, f, args);
	}

	public void i (String f, Object... args)
	{
		logprintf (thecontext, Log.INFO, f, args);
	}

	public void e (String f, Object... args)
	{
		logprintf (thecontext, Log.ERROR, f, args);
	}

	public void wtf (String f, Object... args)
	{
		logprintf (thecontext, Log.ASSERT, f, args);
	}

	public void v (String f, Object... args)
	{
		logprintf (thecontext, Log.VERBOSE, f, args);
	}

	public void d (String f, Object... args)
	{
		logprintf (thecontext, Log.DEBUG, f, args);
	}

	public void d_e (Throwable th)
	{
		log_d_e (thecontext, th);
	}

	public void w (String f, Object... args)
	{
		logprintf (thecontext, Log.WARN, f, args);
	}


	public static class silent extends Q
	{
		private silent() {}
		public silent(Context c) { }
		@Override public void d (String f, Object... args) { }
		@Override public void i (String f, Object... args) { }
		@Override public void w (String f, Object... args) { }
		@Override public void e (String f, Object... args) { }
		@Override public void v (String f, Object... args) { }
		@Override public void wtf (String f, Object... args) { }
		@Override public void d_e (Throwable th) { }
		@Override public void tprint (String s) {}
		@Override public void tprintf (String f, Object... args) {}
		@Override public void tp (String f, Object... args) {}
	}
};
