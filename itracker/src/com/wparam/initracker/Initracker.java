package com.wparam.initracker;

import android.app.Activity;
import android.os.Bundle;
import java.nio.channels.DatagramChannel;
import java.lang.Runnable;
import android.os.Handler;
import android.widget.Button;
import java.net.DatagramSocket;
import java.net.SocketAddress;
import java.net.InetSocketAddress;
import 	java.nio.ByteBuffer;
import android.view.View;
import android.widget.TextView;
import 	java.io.StringReader;
import android.widget.LinearLayout;
import java.io.BufferedReader;
import java.util.LinkedList;
import 	android.os.Vibrator;
import android.content.Context;
import android.widget.ProgressBar;
import android.view.ViewGroup;
import android.graphics.drawable.Drawable;
import android.graphics.drawable.ColorDrawable;


public class Initracker extends Activity
{
    public static final int PORT_NUMBER = 38727;
    Handler hand;
    BackThread bt;
    //int lastInit = -1;
    int lastVib = -1;
    //Drawable[] pbs;
    static final int RED = 0;
    static final int YELLOW = 1;
    static final int GREEN = 2;
    static final int DEATH = 3;
    
    /** Called when the activity is first created. */
    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        if (hand == null)
            hand = new Handler ();

        Q.d (this, "onCreate");

        if (bt == null)
        {
            bt = new BackThread();
            bt.hand = hand;
            bt.a = this;
            bt.q = new Q(this);
            bt.start ();
        }

        /*pbs = new int[4];
        pbs[RED] = getResources().getDrawable (R.drawable.pb_red);
        pbs[YELLOW] = getResources().getDrawable (R.drawable.pb_yellow);
        pbs[GREEN] = getResources().getDrawable (R.drawable.pb_green);
        pbs[DEATH] = getResources().getDrawable (R.drawable.pb_death);*/

        super.onCreate(savedInstanceState);
        setContentView(R.layout.main);
    }

    @Override protected void onPause()
    {
        super.onPause();
        Q.d (this, "onPause");
    }

    @Override protected void onResume()
    {
        super.onResume();
        Q.d (this, "onResume");
    }

    @Override protected void onDestroy()
    {
        super.onDestroy();
        Q.d (this, "onDestroy");

        bt.interrupt ();
        try
        {
            bt.join ();
        }
        catch (Exception e)
        {
            // whatever
        }
        bt = null;
    }

    public ProgressBar getHPBar (String desc)
    {
        ProgressBar pb;
        Drawable d;
        int index, dindex;

        index = Integer.valueOf (desc.substring (3));

        dindex = R.drawable.pb_green;
        if (index < 40)
            dindex = R.drawable.pb_yellow;
        if (index < 15)
            dindex = R.drawable.pb_red;
        if (index < 0)
        {
            dindex = R.drawable.pb_death;
            index *= -1;
        }

        d = getResources().getDrawable (dindex);

        pb = new ProgressBar(this, null, 
                    android.R.attr.progressBarStyleHorizontal);
            pb.setIndeterminate (false);
            pb.setProgressDrawable (d);
            pb.setMax (100);
            pb.setProgress (index);

        return pb;
    }

    public void getInitiative (String startline, BufferedReader br) 
        throws Exception
    {
        LinearLayout ll;
        TextView tv;
        String line;
        ProgressBar pb;
        LinearLayout newlay;
        int width;


        // Clear the box;
        ll = (LinearLayout)findViewById (R.id.outlist);
        ll.removeViews (0, ll.getChildCount ());
        width = ll.getWidth () / 2;
        Q.d (this, "width: %d", width);

        while ((line = br.readLine ()) != null)
        {
            if (line.startsWith ("End"))
                break;

            if (line.startsWith ("Turn"))
            {
                tv = new TextView (this);
                tv.setText ("**" + line);
                ll.addView (tv);
            }
            else
            {
                String up;
                String[] flog = line.split(";");
                int x;

                newlay = new LinearLayout (this);
                newlay.setOrientation (LinearLayout.HORIZONTAL);
                tv = null; pb = null;

                up = flog[0];
                for (x=1;x<flog.length;x++)
                {
                    if (flog[x].startsWith ("HP"))
                    {
                        //up += " " + flog[x];

                        pb = getHPBar (flog[x]);
                    }
                    else if (flog[x].startsWith ("IsCasting"))
                    {
                        if (flog[x].endsWith ("1"))
                            up += " " + "(Casting)";
                    }
                    else if (flog[x].startsWith ("IsDead"))
                    {
                        if (flog[x].endsWith ("1"))
                            up += " " + "(DEAD)";
                    }
                }

                ViewGroup.LayoutParams lp = new ViewGroup.LayoutParams (
                        width,
                        ViewGroup.LayoutParams.WRAP_CONTENT);

                tv = new TextView(this);
                tv.setText (up);

                newlay.addView (tv, lp);

                if (pb != null)
                    newlay.addView (pb, lp);

                newlay.setPadding (1, 1, -5, 1);
                ll.addView (newlay);

            }

        }
    }

    void getVibrate (String startline, BufferedReader br) throws Exception
    {
        long index;
        String line;
        LinkedList<Long> bob = new LinkedList<Long>();
        Q q = new Q(this);

        q.d (this, "Vibrate happening");

        // "Vibrate " --8
        index = Long.valueOf (startline.substring (8));
        if (index <= lastVib)
        {
            q.d ("Aborting; already did this pattern");
            return;
        }

        lastVib = (int)index;

        while ((line = br.readLine ()) != null)
        {
            if (line.compareTo ("End") == 0)
                break;

            bob.add (Long.valueOf (line));
        }

        /* What the fucking fuck, java.  What the fucking fuck */
        Object[] ar = (Object[])bob.toArray();
        long[]bitch = new long[ar.length];
        int x;
        for (x=0;x<ar.length;x++)
        {
            bitch[x] = (Long)(ar[x]);
        }

        q.d (this, "Vibrate: %i", ar.length);
        if (ar.length % 2 == 0 && ar.length > 0)
        {
            Vibrator v = (Vibrator)getSystemService (Context.VIBRATOR_SERVICE);

            v.vibrate (bitch, -1);
        }

    }

    public void takeUpdate (String s)
    {
        StringReader sr;
        BufferedReader br;
        String line;
        Q q = new Q(this);

        sr = new StringReader (s);
        br = new BufferedReader (sr);

        try
        {

            line = br.readLine();

            if (line.startsWith ("Initiative", 0))
            {
                getInitiative (line, br);
            }
            else if (line.startsWith ("Vibrate", 0))
            {
                getVibrate (line, br);
            }
            else
            {
                q.d ("Invalid Message!");
            }

            
        }
        catch (Exception e)
        {
            q.d ("Result: Exception %s", e.toString ());
        }



        /*tv = (TextView)findViewById (R.id.output);
        tv.setText (s);*/
    }

    public void testButton (View b)
    {
        finish ();
    }

    public class NotifyDealie implements Runnable
    {
        public String msg;
        Initracker act;

        public NotifyDealie() {}
        public NotifyDealie(Initracker a, String q)
        {
            msg = q;
            act = a;
        }

        public void run()
        {
            act.takeUpdate (msg);
        }
    }


    public class BackThread extends Thread
    {
        public Handler hand;
        public Initracker a;
        public Q q;

        public void run()
        {
            try
            {

                SocketAddress addr = new InetSocketAddress (PORT_NUMBER);
                DatagramChannel dc = DatagramChannel.open ();
                DatagramSocket sock = dc.socket ();
                sock.bind (addr);
                sock.setReuseAddress (true);
                ByteBuffer javasucks = ByteBuffer.allocate (8192);
                byte[] temp;
                String last = null, newstr;

                q.d ("Listen thread starting");

                while (true)
                {
                    javasucks.clear ();
                    addr = dc.receive (javasucks);
                    if (addr == null)
                    {
                        q.d ("Empty receive fail");
                        continue;
                    }

                    /*q.d ("pos %d rem %d", javasucks.position(), 
                     * javasucks.remaining());*/
                    temp = new byte[javasucks.position()];
                    javasucks.position (0);
                    javasucks.get(temp);

                    newstr = new String (temp);

                    if (last != null && newstr.compareTo(last) == 0)
                    {
                        q.d ("No change, yo.");
                        continue;
                    }

                    last = newstr;

                    NotifyDealie nd = new NotifyDealie(a, newstr);
                    //q.d ("Message received: " + nd.msg);
                    hand.post (nd);
                }



            }
            catch (Exception e)
            {
                q.d ("Well fuck that shit " + e.toString ());
            }
            q.d ("PEACE OUT Y'ALL");

        }
    }
}
