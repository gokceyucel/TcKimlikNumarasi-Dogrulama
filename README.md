# TcKimlikNumarasi-Dogrulama
Dünyanın en basit TC kimlik numarası doğrulama aracı

# Kullanım
```c#
  using TurkishCitizenIdValidator;

  var response = new TurkishCitizenIdentity(12345678901, "AD", "SOYAD", 1900).IsValid();
```