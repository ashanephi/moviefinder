from selenium import webdriver
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.common.by import By
from selenium.webdriver.common.keys import Keys
from selenium.webdriver.common.action_chains import ActionChains
from selenium.webdriver.common.actions.wheel_input import ScrollOrigin
from webdriver_manager.chrome import ChromeDriverManager
import time
import sys

if len(sys.argv) < 2:
    print("Error: No image path provided.")
    sys.exit(1)

image_path = sys.argv[1]


chrome_options = Options()
chrome_options.add_argument("--user-data-dir=C:/Users/nephi/AppData/Local/Google/Chrome/User Data")  # ✅ Use your Chrome profile
chrome_options.add_argument("--profile-directory=Default")  # ✅ Default profile

# ✅ Auto-install correct ChromeDriver version
service = Service(ChromeDriverManager().install())
driver = webdriver.Chrome(service=service)

try:
    driver.get("https://lens.google.com/")

    # Manually solve CAPTCHA if needed

    upload_btn = driver.find_element(By.XPATH, "//input[@type='file']")
    upload_btn.send_keys(image_path)

    input("Solve CAPTCHA if shown, then press Enter...")


    time.sleep(5)  

    # ✅ Print all visible text in Google Lens
    all_text_elements = driver.find_elements(By.XPATH, "//div[@data-q] | //h2 | //span")
    all_text = [elem.text for elem in all_text_elements]
    print("All Extracted Text:", all_text)

    # ✅ Look for movie title (improve pattern matching)
    movie_title = None
    for text in all_text:
        if "movie" in text.lower() or "film" in text.lower():  # Adjust keyword matching
            movie_title = text
            break  # Stop at first match

    if movie_title:
        print("Detected Movie Title:", movie_title)
    else:
        print("No movie title found.")

except Exception as e:
    print(f"Error: {e}")


finally:
    driver.quit()
