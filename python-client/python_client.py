import requests
import random


API_URL = "http://localhost:5051/api/quotesapi"

# Function to load quotes from the text file
def load_quotes(file_path):
    quotes = []
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read().strip()
        entries = content.split('\n.\n')  # Split quotes by the pattern
        for entry in entries:
            try:
                quote_text, author = entry.rsplit(' -- ', 1)
                quote_text = quote_text.strip().replace('\n', ' ')
                author = author.strip()
                quotes.append({'content': quote_text, 'author': author})
            except ValueError:
                print(f"Skipping invalid entry: {entry}")
    return quotes

# Function to add a new quote to the API
def add_new_quote():
    content = input("Enter quote content: ")
    author = input("Enter author: ")
    quote = {'content': content, 'author': author}
    response = requests.post(API_URL, json=quote)
    if response.status_code == 201:
        print(f"Successfully added quote: {content}")
        refresh_quotes_list()  # Refresh the quotes list after adding a new quote
    else:
        print(f"Failed to add quote. Status code: {response.status_code}")

# Function to get a random quote
def get_random_quote():
    
    response = requests.get(f"{API_URL}?page=1&pageSize=-1")
    if response.status_code == 200:
        quotes = response.json()
        random_quote = random.choice(quotes)
        print(f"Random Quote: {random_quote['content']} -- {random_quote['author']}")
    else:
        print("Failed to fetch quotes")

# Function to bulk add quotes to API from a file ( loaded using a load_quotes function )
def load_and_add_quotes():
    file_path = input("Enter the path to the quotes file: ")
    quotes = load_quotes(file_path)
    for quote in quotes:
        response = requests.post(API_URL, json=quote)
        if response.status_code == 201:
            print(f"Successfully added: {quote['content']}")
        else:
            print(f"Failed to add: {quote['content']} - {response.status_code}")
            
    refresh_quotes_list()  # Refresh the quotes list after adding a new quote
            

# Function to refresh the quotes list by making a GET request to the API. 
# This way we have better control over the GET request occurances (as needed)
def refresh_quotes_list():
    response = requests.get(f"{API_URL}?page=1&pageSize=-1")
    if response.status_code == 200:
        global quotes_in_memory
        quotes_in_memory = response.json()  # Store the updated list of quotes in memory
        print(f"Quotes list refreshed. {len(quotes_in_memory)} quotes available.")
    else:
        print("Failed to refresh the quotes list from the API")




# Main function to interact with the user
def main():
    
    # Initially load quotes from the API
    refresh_quotes_list()
    
    while True:
        print("\n--- Python Client -Menu ---")
        print("1. Load quotes from file")
        print("2. Add a new quote")
        print("3. Display a random quote")
        print("4. Refresh quotes list")
        print("5. Exit")
        
        choice = input("Enter your choice: ")
        
        if choice == '1':
            load_and_add_quotes()
        elif choice == '2':
            add_new_quote()
        elif choice == '3':
            get_random_quote()
        elif choice == '4':
            refresh_quotes_list()  # Option to manually refresh the quotes list
        elif choice == '5':            
            print("Exiting...")
            break
        else:
            print("Invalid choice. Please try again.")

if __name__ == "__main__":
    main()
