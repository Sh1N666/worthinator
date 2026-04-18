# Worthinator

Projekt stanowiący system agregacji danych z zewnętrznych interfejsów API w celu analizy opłacalności oraz statystyk gier wideo. System pobiera dane z serwisów: GG.Deals (informacje cenowe), How Long To Beat (statystyki czasu rozgrywki) oraz Steam Store (wyszukiwanie po identyfikatorze SteamAppId).

## Architektura Systemu

Aplikacja opiera się na architekturze mikroserwisów z wykorzystaniem kontenerów:
* **Frontend (`portal`):** Aplikacja Single Page Application (SPA) zbudowana w frameworku Angular 21, z użyciem menedżera pakietów npm (wersja 11.3.0). Komunikacja ze stroną serwerową realizowana jest przy pomocy gRPC-Web.
* **Backend API (`api-inator`):** Główna usługa serwerowa napisana w środowisku .NET, odpowiedzialna za logikę biznesową i integrację z zewnętrznymi interfejsami. Komunikacja wewnątrz sieci Docker odbywa się z użyciem protokołu HTTP/2.
* **Reverse Proxy (`gateway`):** Usługa oparta na .NET pełniąca rolę bramy sieciowej. Rozwiązuje prefiks ścieżki `/api` kierując żądania do usługi `api-inator`, a pozostały ruch routuje bezpośrednio do aplikacji klienckiej `portal`.
* **Baza Danych (`database`):** Instancja MongoDB w wersji 7.0 z odpowiednio skonfigurowanymi mechanizmami uwierzytelniania i healthcheckiem (`mongosh`).

Diagram bazy danych:

<img width="284" height="710" alt="image" src="https://github.com/user-attachments/assets/6c34f49e-bea0-442b-a857-50f01c095a2b" />


---

## Konfiguracja Zmiennych Środowiskowych (.env)

Przed uruchomieniem aplikacji, konieczne jest utworzenie pliku `.env` w głównym katalogu projektu. Poniżej znajduje się wymagany schemat zmiennych wraz z technicznym opisem przeznaczenia każdej z nich:

```env
# --- Konfiguracja bazy danych MongoDB ---
# Używane do inicjalizacji kontenera bazy oraz stringa połączeniowego
DB_ROOT_USER=           # Nazwa konta administratora (root) dla bazy MongoDB.
DB_ROOT_PASS=           # Hasło dla konta administratora.
DB_CONN_PASS_ENCODED=   # Hasło administratora z kodowaniem URL (URL-encoded), niezbędne do poprawnego sparsowania ConnectionStrings__DefaultConnection.
DB_NAME=                # Nazwa docelowej bazy danych wykorzystywanej przez API.

# --- Konfiguracja portów sieciowych ---
# Mapowanie portów usług wewnątrz i na zewnątrz środowiska Docker
API_PORT=               # Port, na którym usługa api-inator nasłuchuje wewnątrz sieci (np. 5000).
GW_PORT_HTTPS=          # Zewnętrzny port, na którym brama (gateway) wystawia usługę po protokole HTTPS (np. 443 lub 8443).
PORTAL_PORT_INTERNAL=   # Wewnętrzny port aplikacji Angular (domyślnie ng serve używa 4200).
PORTAL_PORT_EXTERNAL=   # Zewnętrzny port dla usługi klienckiej, jeśli wymagany jest bezpośredni dostęp z pominięciem bramy.

# --- Konfiguracja certyfikatów SSL/TLS ---
# Wymagane przez serwer Kestrel do obsługi ruchu HTTPS w usłudze gateway
CERT_PASS=              # Hasło użyte do zabezpieczenia pliku certyfikatu.
CERT_PATH=              # Ścieżka do pliku certyfikatu wewnątrz kontenera (np. /certs/gateway.pfx). Wolumen z hosta montowany jest do katalogu /certs w trybie read-only.

# --- Integracje zewnętrznych API ---
GGDEALS_API_KEY=        # Indywidualny klucz autoryzacyjny dla API GG.Deals.
```

---

## Środowisko Uruchomieniowe (Docker Compose)

Rozwiązanie jest przystosowane do pełnej orkiestracji za pomocą narzędzia Docker Compose, które automatycznie powoła izolowaną sieć `Worthinator-net` i utworzy wolumen dla danych z bazy `mongo-data`.

1.  Upewnij się, że plik `.env` został poprawnie utworzony w głównym katalogu, a zmienne zostały zadeklarowane.
2.  Wygeneruj lokalny certyfikat developerski w formacie `.pfx` i umieść go w katalogu `./certs/` na maszynie hosta. Będzie on odczytywany przez usługę `gateway`.
3.  Zbuduj obrazy i uruchom kontenery w tle poleceniem:
    ```bash
    docker compose up -d --build
    ```
4.  Docker uruchomi usługi w odpowiedniej kolejności. Usługa `api-inator` posiada zdefiniowany parametr `depends_on` powiązany z `database`, oczekując na poprawne wykonanie healthchecka (skrypt weryfikujący `db.adminCommand('ping')`) przed rozpoczęciem inicjalizacji interfejsu.

---

## Lokalne Środowisko Deweloperskie (Dev)

Uruchomienie systemu poza kontenerami wymaga natywnej instalacji środowisk uruchomieniowych (.NET SDK 8.0+, Node.js 18.x+, MongoDB 7.0).

**1. Usługa Bazy Danych:**
Uruchom lokalną instancję MongoDB na domyślnym porcie `27017`. Skonfiguruj identyczne poświadczenia dla bazy, co zadeklarowane w lokalnych zmiennych środowiskowych aplikacji.

**2. Warstwa Backendowa (.NET)**

1. Otwórz plik solucji `worthinator.slnx` w kompatybilnym środowisku IDE.
2. Przed uruchomieniem wyeksportuj wymagane zmienne środowiskowe do sesji terminala lub dodaj je do konfiguracji profilu startowego (pliki `launchSettings.json`).
3. **Konfiguracja środowiska Python:**
   Przed startem projektów musisz przygotować interpreter:
   * Przejdź do katalogu: `src/ApiInator/Infrastructure`
   * Utwórz wirtualne środowisko: `python3.12 -m venv venv`
   * Aktywuj środowisko: `source venv/bin/activate`
   * Zainstaluj zależności: `pip install -r requirements.txt`
4. Uruchom równolegle projekty `ApiInator.csproj` oraz `Frontend.Gateway.csproj`.

**3. Warstwa Kliencka (Angular):**
* Przejdź do podkatalogu zawierającego projekt frontendowy: `cd src/Portal`.
* Pobierz niezbędne zależności poleceniem:
    ```bash
    npm install
    ```
* Wygeneruj kod klientów gRPC z plików definicji (`.proto`) znajdujących się w nadrzędnych katalogach projektu. Służy do tego zdefiniowany w pakiecie skrypt:
    ```bash
    npm run proto
    ```
* Uruchom proces kompilacji w trybie watch mode wywołując:
    ```bash
    npm run start
    ```
