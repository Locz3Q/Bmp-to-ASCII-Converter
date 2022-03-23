; Temat: Konwersja pliku .bmp do ASCII w formacie tekstowym .txt
; Opis algorytmu:
; 	Algorytm przyjmuje wartosci skladowych kazdego piksela, dzieli je przez 30 i sumuje je ze soba tworzac wartosc sredniej.
;	Do otrzymanej sredniej dodaje sie wartosc 39 w celu efektywniejszego podejrzenia otrzymanych wynikow.
;	Powstala wartosc przekazuje sie do pamieci programu.
;
; Autor: 			Blazej Biskup
; Wydzial: 			AEI
; Kierunek: 		Informatyka, SSI
; Grupa: 			3
; Sekcja: 			1
; Semestr: 			5
; Rok Akademicki:	2021/2022
;
; Przebieg powstawania projektu
;      1. Utworzenie pustych bibliotek dll w C++ i Asm x64 oraz pustej aplikacji okienkowej WPF w C#
;      2. Skonstruowanie GUI ze wszystkimi mozliwosciami konfiguracji pracy dzialania programu w czasie rzeczywistym
;      3. Implementacja metod obslugujacych utworzone w poprzednim punkcie checkboxow oraz przyciskow
;      4. Implementacja wielowatkowosci w postaci objektow Task w C#
;      5. Stworzenie algorytmu w C++ oraz przekazywanie odpowiednich parametrow
;      6. Stworzenie algorytmu w Asm x64 oraz przekazywanie odpowiednich parametrow

.DATA

_39 dq 39		; Stala 39

thirty real4 30.0, 30.0, 30.0		; Wartosc 30 uzywana jako dzielnik skladowej piksela

clear db    00h, 00h, 00h, 0FFh,			; Tablica zerujaca 3 pierwsze bajty
				0FFh, 0FFh, 0FFh, 0FFh, 
				0FFh, 0FFh, 0FFh, 0FFh,  
				0FFh, 0FFh, 0FFh, 0FFh

focus db	0FFh, 0FFh, 0FFh, 00h, 00h, 00h, 00h, 00h, 00h, 00h, 00h, 00h, 00h, 00h, 00h, 00h		; Tablica zerujaca 13 ostatnich bajtow zostawiajac 3 pierwsze

Ffocus db	00h, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh		; Maska w formie tablicy do zapisania najm�odszego bajtu na poczatku i wyzerowaniu reszty bajtow

Sfocus db	04h, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh		; Maska w formie tablicy do zapisania czwartego bajtu na poczatku i wyzerowaniu reszty bajtow

Tfocus db	08h, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh, 0FFh		; Maska w formie tablicy do zapisania osmego bajtu na poczatku i wyzerowaniu reszty bajtow

.CODE
ASCIIArt proc
lea rsi, clear			; Zapisanie adresu tablicy zerujacej 3 pierwsze bajty w rsi
movups xmm10, [rsi]		; Zapisanie tablicy zerujacej w xmm10
lea rsi, thirty			; Zapisanie adresu do wartosci 30.0 w rsi
movups xmm4, [rsi]		; Zapisanie wartosci 30.0 w xmm4
lea rsi, focus			; Zapisanie adresu tablicy zerujacej 13 ostatnich bajtow w rsi
movups xmm5, [rsi]		; Zapisanie tablicy zerujacej w xmm5
lea rsi, Ffocus			; Zapisanie adresu maski do zapisania najmlodszego bajtu na poczatku i wyzerowaniu reszty bajtow w rsi
movups xmm12, [rsi]		; Zapisanie maski w xmm12
lea rsi, Sfocus			; Zapisanie adresu maski do zapisania czwartego bajtu na poczatku i wyzerowaniu reszty bajtow w rsi
movups xmm13, [rsi]		; Zapisanie maski w xmm13
lea rsi, Tfocus			; Zapisanie adresu maski do zapisania osmego bajtu na poczatku i wyzerowaniu reszty bajtow w rsi
movups xmm14, [rsi]		; Zapisanie maski w xmm14

ascii:
movups xmm0, [rdx]		; Wczytanie 16 bajtow z tablicy RGB do xmm0
movups xmm1, xmm0		; Skopiowanie wartosci wektora xmm0 do xmm1

pand xmm1, xmm10		; Wyzerowanie pierwszych 3 bajtow wektora xmm1
pand xmm0, xmm5			; Wyzerowanie ostatnich 13 bajtow wektora xmm0
pmovzxbd xmm0, xmm0		; Konwersja xmm0 na podwojne slowa
cvtdq2ps xmm0, xmm0		; Konwersja xmm0 z podwojnych slow na liczby zmiennoprzecinkowe pojedynczej precyzji
divps xmm0, xmm4		; Wykonanie dzielenia przez 30 pierwszych trzech elementow xmm0
cvtps2dq xmm0, xmm0		; Konwersja xmm0 z liczb zmiennoprzecinkowych na podwojne slowa

movups xmm6, xmm0		; Zapisanie kopii xmm0 w trzech innych wektorach xmm6, xmm7, xmm8
movups xmm7, xmm0
movups xmm8, xmm0

pshufb xmm6, xmm12		; Zapisanie 0 bajtu w dolnej polowce wektora i wyzerowanie reszty bajtow
pshufb xmm7, xmm13		; Zapisanie 4 bajtu w dolnej polowce wektora i wyzerowanie reszty bajtow
pshufb xmm8, xmm14		; Zapisanie 8 bajtu w dolnej polowce wektora i wyzerowanie reszty bajtow

movups xmm9, xmm6		; Dodanie Wartosci kazdego wektora i zapisanie go w xmm9
paddd xmm9, xmm7
paddd xmm9, xmm8

addsd xmm9, [_39]		; Dodanie stalej 39 do wektora z wynikiem w celu osiagniecia lepszej jakosci wynikow

punpcklbw xmm9, xmm9	; Duplikacja najmlodszego bajtu wiekszego od 0 i zapisanie ich obok siebie
punpcklbw xmm9, xmm9

paddd xmm9, xmm1		; Dodanie 13 najstarszych bajtow do wyniku
movups [rdx], xmm9		; Zapisanie wyniku w pamieci

add rdx, 3				; Przesuniecie wskaznika tablicy RGB o 3
sub rcx, 3				; Przesuniecie licznika o 3
cmp rcx, 0				; Porownanie zawartosci rcx z zerem
jg ascii				; Ponowne wykonanie petli jesli zawartosc rcx jest wieksza od 0
ret

ASCIIArt endp
END