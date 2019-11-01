package com.example.hung.monitor;

import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.Console;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.net.InetAddress;
import java.net.Socket;
import java.net.UnknownHostException;

import org.json.JSONObject;
import android.app.Activity;
import android.os.Bundle;
import android.os.Handler;
import android.os.HandlerThread;
import android.util.Log;
import android.widget.EditText;
import android.widget.TextView;


//https://developer.android.com/studio/run/emulator-networking.html
public class MainActivity extends AppCompatActivity {
    private Button start;
    private TextView temp;
    private TextView gas;
    private TextView fire;
    private TextView rain;
    private TextView dist;
    private TextView body;
    int serverPort = 7777;
    private Socket clientSocket;
    private BufferedReader br;
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        start = findViewById(R.id.button);
        temp = findViewById(R.id.textView6);
        gas = findViewById(R.id.textView7);
        fire = findViewById(R.id.textView8);
        rain = findViewById(R.id.textView9);
        dist = findViewById(R.id.textView10);
        body = findViewById(R.id.textView11);
        start.setOnClickListener(new Button.OnClickListener(){
            @Override
            public void onClick(View v){
                Thread thread=new Thread(connect);
                thread.start();
            }
        });
    }

    private final Runnable connect = new Runnable() {
        @Override
        public void run() {
            try {
                InetAddress serverIp = InetAddress.getByName(((EditText) findViewById(R.id.editText)).getText().toString());
                clientSocket = new Socket(serverIp, serverPort);
                br = new BufferedReader(new InputStreamReader(clientSocket.getInputStream()));
                while (clientSocket.isConnected()) {
                    String line = br.readLine();
                    if (line != null) {
                        final String[] l = line.split(",");
                        OnNewSensorData(l);
                    }
                    else{
                        runOnUiThread(new Runnable() {
                            @Override
                            public void run() {
                                temp.setText("");
                                gas.setText("");
                                fire.setText("");
                                rain.setText("");
                                dist.setText("");
                                body.setText("");
                                start.setEnabled(true);
                            }
                        });
                    }
                }
            } catch (UnknownHostException e) {
                Log.d("1",e.toString());
            } catch (IOException exe) {
                Log.d("2",exe.toString());
            }
        }
    };

    @Override
    protected void onDestroy(){
        super.onDestroy();
        try {
            br.close();
            clientSocket.close();
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    public void OnNewSensorData(final String[] l) {
        runOnUiThread(new Runnable() {
            public void run() {
                temp.setText(l[0]);
                gas.setText(l[1]);
                fire.setText(l[2]);
                rain.setText(l[3]);
                dist.setText(l[4]);
                body.setText(l[5]);
                start.setEnabled(false);
            }
        });
    }
}
