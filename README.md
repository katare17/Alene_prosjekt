
# Programmeringsprosjekt IS-202
## Katarina Kirkhus - 2025

Dette prosjektet er laget i forbindelse med gjennomføringen av emnet IS-202. Prosjektet skulle være et gruppeprosjekt, men på grunn av uforutsette problemer er det nå laget alene. Det hele er laget i løpet av en drøy måned (altså en dag eller to ekstra) og det har vært litt av en jobb. 

Nå er prosjektet ferdig og det er laget for at Kartverket skal kunne motta og prosessere feilmeldinger fra kartbrukere landet over. Brukerne skal kunne lage egen konto, lage og sende rapporter, slette rapportene sine, logge seg ut, samt slette sin egen konto. De ansatte ved Kartverket (her kalt Caseworkere) skal kunne se innmeldte rapporter, prosessere disse (avslå eller godkjenne), slette brukere og logge seg ut. Alt dette kan man se dersom man starter opp prosjektet. 

## Lenker til resten av dokumentet
Lenke til [API-bruk](#api-bruk).

Lenke til [Installasjon](#installasjon).

Lenke til [Caseworker login](#caseworkerlogin)

Lenke til [Testing](#testing).

Lenke til [Forbedringspotensiale](#forbedringspotensiale).

Lenke til [Kilder](#kilder).


## API-bruk

Api-bruken i dette prosjektet bygger på Caseworkere sin egen interesse av å kunne se hvor rapportens feil ligger (hhv. i hvilken kommune og hvilket fylke). Dette gjør det lettere for Caseworkerne å forstå akkurat hvor i landet feilene ligger. Ved videre utvikling vil det også kunne legges til rette for at rapportene kan sorteres etter hvilket fylke eller hvilken kommune de ligger i. Kanskje vil det være interessant for Kartverket å vite hvilken kommune eller hvilket fylke som har flest innmeldte feil?


## Installasjon

### 1. steg
I din egen git-terminal, gå inn i mappen du ønsker å ha prosjektet i:

```bash
  git clone https://github.com/katare17/Alene_prosjekt.git
```
Åpne Visual Studio 2022 og finn frem løsningen.

Åpne filen kalt "WebApplication1.sln"

---
### 2. steg
Når prosjektet er åpnet i Visual Studio:

I Visual Studio, last ned *dotConnect for MySQL & MariaDB* (utgitt av: *Devart Software*)
```bash
  Extensions -> Manage Extensions -> dotConnect for MySQL & MariaDB
```
Nå må du restarte programmet

---
### 3. steg
Øverst i programmet, klikker du på
```bash
  Build -> Build solution 
```

---
### 4. steg 

På nest øverste rad i programmet, klikker du på
```bash
  Docker Compose
```
<sup> Den er rett ved den grønne "play-knappen"</sup>

---
### 5. steg

Øverst i programmet, klikk på:
```bash
  View -> Server Explorer 
```

Høyreklikk på:
```bash
  Data Connections
```

Klikk på: 
```bash
  Add Connection
```
Velg:
```bash
  MySQLServer
```
Legg inn følgende informasjon:
```bash
  Host = localhost
  Port = 3306
  User Id = root
  Password = 123
  Database = geochangesdb 
```

---
### 6. steg
Nederst i programmet, trykk på 
```bash
Package Manager Console
```

Skriv inn:
```bash
Drop-Database
```
Trykk på enter, så skriv inn:
```bash
Update-Database
```
Trykk på enter

---
### 7. steg
Klikk igjen på 
```bash
  Docker Compose
```

---

### Caseworkerlogin
Brukernavn: Caseworker@test <br/>
Passord: Test1

### Funker ikke applikasjonen?
Spør ChatGPT om hjelp ;*

---
## Testing

I programmet er det lagt til to Unit Tester her:
```bash
  WebApplicationTests/ControllerTests/AccountControllerTests
```

*Den første testen* sjekker at når en rapport lages og mangler eller har feil i GeoJSON-input eller beskrivelses-input, så kommer den tilbake med en badRequest.

*Den andre testen* sjekker at når en rapport lages uten en tilknyttet bruker-ID, så kommer den tilbake med en unauthorizedResult.

<br/>

*Videre* er det også gjort ***mye*** brukertesting underveis i arbeidet med prosjektet. Det er også testet på andre personers datamaskiner, for sikkerhetsskyld - og resultatet var at det fungerte hos samtlige (referanser kan oppgis ved etterspørsel). 


## Forbedringspotensiale

Ettersom prosjektet er laget på én måned, er det en del forbedringspotensiale ute og går. Hadde jeg hatt bedre tid, eller arbeidet sammen med flere, ville det nok blitt lagt til følgende elementer/funksjonaliteter:
* Legge inn slik at endringer innen status på rapporter ikke endret view, men forble der den var
* Legge inn en slags "undo-funksjon" på rapportene som er satt til enten "godkjent" eller "avslått", slik at de kom tilbake på oversikten over ubehandlede rapporter
* Gjort slik at kartet zoomet ut nok til at alle markeringene ble synlige når man kikket på en av oversiktssidene
* Gjort slik at den aktuelle markeringen i kartet enten var den eneste igjen på kartet eller lyste opp når man "hovrer" over en rapportlinje
* Laget flere tester
* Lagt inn Kartverkets logo i nav-baren
* Flyttet noen av nav-bar elementene helt til høyre - og kanskje noen inn i en burgermeny
* Sikkert enda mer jeg enda ikke har tenkt på

## Kilder

I utformingen av dette prosjektet har følgende kilder blitt brukt til maler, inspirasjon og hjelp:

* Tildelt fagstoff
* Andres offentlige koder
* ChatGPT
* GitHub Copilot
* Personlig kommunikasjon med læringsassistenter
* Bootstrap
* W3Schools
* StackOverflow
* Personlig kommunikasjon med andre som har bestått emnet før
