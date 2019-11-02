package com.example.hung.monitor;
import android.app.Notification;
import android.app.NotificationManager;
import android.content.Context;
import android.graphics.Color;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.InetAddress;
import java.net.Socket;
import java.net.UnknownHostException;
import android.util.Log;
import android.widget.EditText;
import android.widget.TextView;
import android.support.v4.app.NotificationCompat;
//https://developer.android.com/studio/run/emulator-networking.html
public class MainActivity extends AppCompatActivity {
    private Button start;
    private Button end;
    private TextView temp;
    private TextView gas;
    private TextView fire;
    private TextView rain;
    private TextView dist;
    private TextView body;
    int serverPort = 7777;
    private Socket clientSocket;
    private BufferedReader br;
    private Thread thread;
    private ToastHandler th;
    private String alarm_title;
    private String alarm_message;
    private Notification notify;
    private NotificationManager Nm;
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        start = findViewById(R.id.button);
        end = findViewById(R.id.button2);
        end.setEnabled(false);
        temp = findViewById(R.id.textView6);
        gas = findViewById(R.id.textView7);
        fire = findViewById(R.id.textView8);
        rain = findViewById(R.id.textView9);
        dist = findViewById(R.id.textView10);
        body = findViewById(R.id.textView11);
        th = new ToastHandler(getApplicationContext());
        start.setOnClickListener(press_start);
        end.setOnClickListener(press_end);
        Nm = (NotificationManager) getSystemService(Context.NOTIFICATION_SERVICE); // 取得系統的通知服務
        notify = new Notification.Builder(getApplicationContext()).setSmallIcon(R.drawable.ic_launcher_foreground).setContentTitle(alarm_title).setContentText(alarm_message).build();
    }

    private final Runnable connect = new Runnable() {
        @Override
        public void run() {
            try {
                InetAddress serverIp = InetAddress.getByName(((EditText) findViewById(R.id.editText)).getText().toString());
                clientSocket = new Socket(serverIp, serverPort);
                br = new BufferedReader(new InputStreamReader(clientSocket.getInputStream()));
                runOnUiThread(new Runnable() {
                    @Override
                    public void run() {
                        end.setEnabled(true);
                        start.setEnabled(false);
                    }
                });
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
                                end.setEnabled(false);
                            }
                        });
                        th.showToast("連線中斷",3);
                        clientSocket.close();
                        return;
                    }
                }
            } catch (UnknownHostException e) {
                th.showToast("連線失敗",3);
                Log.d("1",e.toString());
            } catch (IOException exe) {
                th.showToast("連線失敗",3);
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
                if (Integer.parseInt(l[1])==0)
                    temp.setTextColor(Color.GREEN);
                else {
                    temp.setTextColor(Color.RED);
                    show_notification("溫度過高", "");
                }
                if (Integer.parseInt(l[3])==0) {
                    gas.setText("正常");
                    gas.setTextColor(Color.GREEN);
                }
                else {
                    gas.setText("過高");
                    gas.setTextColor(Color.RED);
                    show_notification("溫度過高", "");
                }
                if (Integer.parseInt(l[5])==0){
                    fire.setText("無");
                    fire.setTextColor(Color.GREEN);
                }
                else {
                    fire.setText("火光反應");
                    fire.setTextColor(Color.RED);
                    show_notification("溫度過高", "");
                }
                if (Integer.parseInt(l[7])==0){
                    rain.setText("無");
                    rain.setTextColor(Color.GREEN);
                }
                else {
                    rain.setText("有");
                    rain.setTextColor(Color.RED);
                    show_notification("溫度過高", "");
                }
                if (Integer.parseInt(l[9])==0) {
                    dist.setText("門關閉");
                    dist.setTextColor(Color.GREEN);
                }
                else {
                    dist.setText("門打開");
                    dist.setTextColor(Color.RED);
                    show_notification("溫度過高", "");
                }
                if (Integer.parseInt(l[11])==0) {
                    body.setText("無反應");
                    body.setTextColor(Color.GREEN);
                }
                else {
                    body.setText("有反應");
                    body.setTextColor(Color.RED);
                    show_notification("溫度過高", "");
                }
            }
        });
    }

    public void show_notification(String title , String m){
        this.alarm_title=title;
        this.alarm_message = m;
        Nm.notify(0, notify);
        Log.i("dfdf","sdfdsf");
    }

    private View.OnClickListener press_start = new View.OnClickListener() {
        @Override
        public void onClick(View v) {
            thread=new Thread(connect);
            thread.start();
        }
    };
    private View.OnClickListener press_end = new View.OnClickListener() {
        @Override
        public void onClick(View v) {
            try {
                clientSocket.close();
                end.setEnabled(false);
                start.setEnabled(true);
                runOnUiThread(new Runnable() {
                    @Override
                    public void run() {
                        th.showToast("連線中斷",3);
                        temp.setText("");
                        gas.setText("");
                        fire.setText("");
                        rain.setText("");
                        dist.setText("");
                        body.setText("");
                        start.setEnabled(true);
                    }
                });
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
    };
}
