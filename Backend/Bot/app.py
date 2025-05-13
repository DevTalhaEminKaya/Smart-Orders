import pika
import time
import json
import gzip
import os

from selenium import webdriver
from selenium.webdriver import FirefoxOptions
from selenium.webdriver.common.by import By
from selenium.webdriver.common.keys import Keys
from seleniumwire import webdriver

def trendyol(username, password):
    firefox_options = FirefoxOptions()
    firefox_options.add_argument("--headless")
    
    seleniumwire_options = {
        'disable_capture': False,
        'enable_har': False
    }

    driver = webdriver.Firefox(
        options=firefox_options,
        seleniumwire_options=seleniumwire_options,
    )

    try:
        driver.requests.clear()
        driver.get("https://www.trendyol.com/Login")
        time.sleep(1)

        email_field = driver.find_element(By.ID, "login-email")
        password_field = driver.find_element(By.ID, "login-password-input")

        email_field.send_keys(username)
        password_field.send_keys(password + Keys.RETURN)
        time.sleep(1)

        driver.get("https://www.trendyol.com/hesabim/siparislerim")
        time.sleep(1)

        found_order_data = False
        for request in driver.requests:
            if "orders?page" in request.url.lower() and request.response:
                try:
                    body = request.response.body
                    
                    if request.response.headers.get('Content-Encoding') == 'gzip':
                        body = gzip.decompress(body)
                    
                    response_json = json.loads(body.decode('utf-8'))
                    print(json.dumps(response_json, indent=4, ensure_ascii=False))
                    found_order_data = True
                    
                except Exception as e:
                    print(f"Response işleme hatası: {str(e)}")
        
        if not found_order_data:
            print("Sipariş verisi bulunamadı!")

    except Exception as e:
        print(f"Genel hata: {str(e)}")
    
    finally:
        driver.quit()

def callback(ch, method, properties, body):
    try:       
        task = json.loads(body.decode())
        
        if not all(key in task for key in ["Website","Username","Password"]):
            raise ValueError("Geçersiz mesaj formatı")
            
        website = task["Website"]
        username = task["Username"]
        password = task["Password"]

        print(f" [x] {website} isteği işleniyor...")
        
        if website == "trendyol":
            trendyol(username, password)
        else:
            print(f"Desteklenmeyen site: {website}")

        ch.basic_ack(delivery_tag=method.delivery_tag)
            
    except Exception as e:
        print(f"Callback hatası: {str(e)}")

time.sleep(2)

print(" [-] Bot çalışıyor, görev bekleniyor.")

connection = pika.BlockingConnection(
    pika.ConnectionParameters(
        host='localhost',
        port=5672,
        credentials=pika.PlainCredentials('guest', 'guest')
    )
)

channel = connection.channel()
channel.queue_declare(queue='bot_queue')
channel.basic_qos(prefetch_count=1)

channel.basic_consume(queue='bot_queue', on_message_callback=callback)

channel.start_consuming()