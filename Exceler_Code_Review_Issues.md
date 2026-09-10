# 🔍 گزارش جامع بررسی کد و ایرادات فریم‌ورک Exceler
**تاریخ بررسی:** سپتامبر ۲۰۲۶  
**مخزن:** [Exceler (GitHub)](https://github.com/AmirMahdiyar/Exceler)  
**نسخه:** `1.0.0-beta.1`  
**تارگت فریم‌ورک‌ها:** `.NET 6.0`, `.NET 7.0`, `.NET 8.0`, `.NET 9.0`  

---

## 📋 فهرست مطالب
1. [خلاصه ارزیابی مهندسی](#خلاصه-ارزیابی-مهندسی)
2. [ایرادات بحرانی و معماری (Critical & Architectural)](#۱-ایرادات-بحرانی-و-معماری-critical--architectural)
3. [ایرادات خط لوله و نقض اصول SOLID (Design & SOLID Flaws)](#۲-ایرادات-خط-لوله-و-نقض-اصول-solid-design--solid-flaws)
4. [ایرادات کارایی و مدیریت حافظه (Performance & Memory)](#۳-ایرادات-کارایی-و-مدیریت-حافظه-performance--memory)
5. [ایرادات قابلیت اطمینان، انواع داده و تبدیل‌ها (Reliability & Data Types)](#۴-ایرادات-قابلیت-اطمینان-انواع-داده-و-تبدیل‌ها-reliability--data-types)
6. [ایرادات طراحی API و تجربه توسعه‌دهنده (API Design & DX)](#۵-ایرادات-طراحی-api-و-تجربه-توسعه‌دهنده-api-design--dx)
7. [ایرادات و خلأهای پروژه تست (Testing Gaps)](#۶-ایرادات-و-خلأهای-پروژه-تست-testing-gaps)
8. [جدول ماتریس اولویت‌بندی رفع اشکالات](#۷-جدول-ماتریس-اولویت‌بندی-رفع-اشکالات)

---

## خلاصه ارزیابی مهندسی
پکیج **Exceler** ایده‌های مدرن و نوآورانه‌ای مانند **کامپایل Expression Trees به جای Reflection** و **پایپ‌لاین پردازش سطری** را برای کار با فایل‌های اکسل در دات‌نت ارائه کرده است. با این وجود، به عنوان یک محصول نرم‌افزاری آماده انتشار در محیط‌های عملیاتی (Production)، با ایرادات جدی در حوزه‌های لایسنسینگ، کارایی واقعی، پشتیبانی از سیستم‌عامل‌های غیر ویندوزی، قابلیت اطمینان تبدیل داده‌ها و تجربه توسعه‌دهنده مواجه است.

---

## ۱. ایرادات بحرانی و معماری (Critical & Architectural)

### ۱.۱. باگ بحرانی در بازنویسی لایسنس تجاری EPPlus
* **سطح اهمیت:** 🔴 بحرانی (Critical / Legal Risk)
* **فایل‌های متاثر:** 
  - `src/Exceler/Core/DefaultReader.cs` (خط ۲۵ و ۱۲۷)
  - `src/Exceler/Core/DefaultWriter.cs` (خط ۲۲)
  - `src/Exceler/DependencyInjection/ExcelerBuilder.cs` (خط ۶۶)
* **شرح ایراد:**
  در تنظیمات اولیه، متد `builder.UseCommercialLicense()` برای کسب‌وکارهای دارای لایسنس تجاری تعبیه شده است. اما در سازنده کلاس‌های `DefaultReader`، `DefaultWriter` و درون متد `ResolveDependencies`، مقدار زیر هاردکد شده است:
  ```csharp
  ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
  ```
* **پیامد:**
  هر بار که عملیات خواندن یا نوشتن انجام شود، لایسنس مجدداً به صورت پنهانی به `NonCommercial` تغییر می‌کند. این یک **باگ حقوقی و نقض شرایط لایسنسینگ EPPlus** برای مشتریان تجاری است.
* **راهکار پیشنهادی:**
  حذف هاردکدهای درون کلاس‌های Reader و Writer و احترام به مقدار تنظیم‌شده در `ExcelerBuilder` یا تزریق `ExcelerOptions`.

---

### ۱.۲. ضدالگوی Service Locator در کلاس‌های Core
* **سطح اهمیت:** 🟠 بالا (High)
* **فایل‌های متاثر:** 
  - `src/Exceler/Core/DefaultReader.cs` (خطوط ۱۹، ۱۲۴-۱۳۲)
  - `src/Exceler/Core/DefaultWriter.cs` (خطوط ۱۹، ۹۰-۹۵)
* **شرح ایراد:**
  کلاس‌های اصلی به جای دریافت وابستگی‌های صریح، اینترفیس `IServiceProvider` را تزریق کرده و در متدها به شکل دستی سرویس‌ها را Resolve می‌کنند:
  ```csharp
  var profile = _serviceProvider.GetRequiredService<ExcelProfile<TInput>>();
  var processor = _serviceProvider.GetRequiredService<IExcelProcessor<TInput, TOutput>>();
  var validator = _serviceProvider.GetService<IExcelValidator<TInput>>();
  ```
* **پیامد:**
  - نقض صریح اصل Dependency Inversion (DIP).
  - پنهان ماندن وابستگی‌ها و عدم امکان تست ساده بدون شبیه‌سازی کامل DI Container.
  - ریسک خطای Captive Dependencies در زمان فراخوانی از Scopes ناهمخوان.
* **راهکار پیشنهادی:**
  استفاده از Factory اختصاصی یا رجیستری مرکزی سرویس‌های اکسل برای مدیریت Lifecycle مناسب.

---

### ۱.۳. محدودیت یک پروفایل به ازای هر مدل داده
* **سطح اهمیت:** 🟠 بالا (High)
* **فایل‌های متاثر:** 
  - `src/Exceler/DependencyInjection/ExcelerBuilder.cs` (خطوط ۷۷-۸۴)
* **شرح ایراد:**
  نحوه ثبت پروفایل‌ها در DI به این صورت است:
  ```csharp
  var profileBaseType = GetBaseTypeOfRawGeneric(type, typeof(ExcelProfile<>));
  if (profileBaseType != null)
  {
      Services.AddSingleton(profileBaseType, type);
  }
  ```
* **پیامد:**
  اگر در سامانه برای کلاس `Employee` دو سناریو وجود داشته باشد (مثلاً `EmployeeSimpleProfile` برای خروجی اکسل سریع و `EmployeeFullProfile` برای ایمپورت منابع انسانی)، هر دو به عنوان `ExcelProfile<Employee>` ثبت شده و دومی اولی را بدون اطلاع حذف و Overwrite می‌کند!
* **راهکار پیشنهادی:**
  پشتیبانی از Named Profiles یا امکان پاس دادن مستقیم نوع پروفایل به متدهای خواندن و نوشتن (مانند `Read<TInput, TOutput, TProfile>()`).

---

## ۲. ایرادات خط لوله و نقض اصول SOLID (Design & SOLID Flaws)

### ۲.۱. بسته بودن خط لوله در برابر توسعه (نقض Open/Closed Principle)
* **سطح اهمیت:** 🟠 بالا (High)
* **فایل‌های متاثر:** 
  - `src/Exceler/Core/DefaultReader.cs` (خطوط ۱۸۴-۱۹۲)
  - `src/Exceler/Core/DefaultWriter.cs` (خطوط ۷۹-۸۸)
* **شرح ایراد:**
  زنجیره هندلرها در متد `BuildProcessingChain` به شکل صلب با `new` ساخته می‌شود:
  ```csharp
  var head = new ParseHandler<TInput, TOutput>();
  head.SetNext(new ValidateHandler<TInput, TOutput>())
      .SetNext(new ProcessHandler<TInput, TOutput>());
  ```
* **پیامد:**
  برخلاف ادعای مستندات، کاربر نمی‌تواند هیچ Middleware یا Handler اختصاصی (مانند لاگین، رمزنگاری، هش کردن مقادیر یا اعتبارسنجی خارجی) به پایپ‌لاین بیفزاید.
* **راهکار پیشنهادی:**
  امکان تعریف و تزریق زنجیره هندلرها از طریق DI و متد کانفیگ `AddExcelCore(b => b.AddReadHandler<CustomHandler>())`.

---

### ۲.۲. اجبار به پیاده‌سازی `IExcelProcessor` برای سناریوهای ابتدایی
* **سطح اهمیت:** 🟡 متوسط (Medium)
* **فایل‌های متاثر:** 
  - `src/Exceler/Core/DefaultReader.cs` (خط ۱۲۹)
* **شرح ایراد:**
  متد `ResolveDependencies` از `GetRequiredService<IExcelProcessor<TInput, TOutput>>()` استفاده می‌کند.
* **پیامد:**
  اگر برنامه‌نویس بخواهد یک شیت ساده را داخل کلاس `User` بخواند (`Read<User, User>`)، حتی بدون هیچ لاجیک بیزنسی باز هم مجبور است یک کلاس جداگانه پیاده‌سازی کرده و در DI ثبت کند، در غیر این صورت برنامه در زمان اجرا کرش می‌کند.
* **راهکار پیشنهادی:**
  تعریف یک `DefaultPassThroughProcessor<T>` در صورتی که پردازنده‌ای در DI ثبت نشده باشد.

---

### ۲.۳. همگام بودن (Synchronous) پردازنده‌ها و اعتبارسنج‌ها
* **سطح اهمیت:** 🟠 بالا (High)
* **فایل‌های متاثر:** 
  - `src/Exceler/Abstractions/IExcelValidator.cs`
  - `src/Exceler/Abstractions/IExcelProcessor.cs`
* **شرح ایراد:**
  امضای متدها کاملاً سنکرون است:
  ```csharp
  IEnumerable<string> Validate(TInput input);
  TOutput Process(TInput input);
  ```
* **پیامد:**
  در سناریوهای واقعی سازمانی، اعتبارسنجی ردیف اکسل مستلزم چک کردن دیتابیس است (مثل بررسی تکراری نبودن شماره شبا یا کد ملی). سنکرون بودن اینترفیس توسعه‌دهنده را مجبور به استفاده از `.Result` یا `Task.Run().GetAwaiter().GetResult()` می‌کند که منجر به **Thread Pool Starvation و Deadlock** در وب‌اپلیکیشن‌ها می‌شود.
* **راهکار پیشنهادی:**
  افزودن اینترفیس‌های ناهمگام مانند `IAsyncExcelValidator` و `IAsyncExcelProcessor`.

---

### ۲.۴. محدودیت غیرمنطقی `where TModel : class, new()` در رایتر
* **سطح اهمیت:** 🟡 متوسط (Medium)
* **فایل‌های متاثر:** 
  - `src/Exceler/Abstractions/IExcelWriter.cs`
  - `src/Exceler/Core/DefaultWriter.cs`
* **شرح ایراد:**
  تمام متدهای نوشتن اکسل قید `new()` دارند:
  ```csharp
  Task WriteAsync<TModel>(IEnumerable<TModel> data, ...) where TModel : class, new();
  ```
* **پیامد:**
  زمان خروجی گرفتن، اشیاء از قبل ایجاد شده‌اند و هیچ نیازی به سازنده پیش‌فرض نیست. این محدودیت مانع از ارسال `record`های دارای پوزیشنال کانستراکتور، مدل‌های تغییرناپذیر (Immutable) و DDD Entities می‌شود.

---

## ۳. ایرادات کارایی و مدیریت حافظه (Performance & Memory)

### ۳.۱. عدم انطباق ادعای "Zero-Allocation" با واقعیت لود EPPlus
* **سطح اهمیت:** 🟠 بالا (High)
* **فایل‌های متاثر:** 
  - `src/Exceler/Core/DefaultReader.cs` (خط ۷۷)
  - `src/Exceler/Core/DefaultWriter.cs` (خط ۳۱، ۴۲)
* **شرح ایراد:**
  کتابخانه EPPlus بر مبنای DOM در حافظه کار می‌کند. کد زیر:
  ```csharp
  await package.LoadAsync(excelStream, cancellationToken);
  ```
  کل فایل اکسل را به صورت یکپارچه در RAM بارگذاری می‌کند. برای فایل‌های بالای ۱۰۰ هزار سطر، حافظه مورد نیاز ده‌ها برابر اندازه فایل فیزیکی در رم مصرف می‌شود. چانک‌بندی خروجی صرفاً تخصیص لیست‌های خروجی C# را به تعویق می‌اندازد اما مصرف RAM پکیج زیرین را کاهش نمی‌دهد.

---

### ۳.۲. سربار فلج‌کننده `AutoFitColumns` در فایل‌های حجیم
* **سطح اهمیت:** 🟠 بالا (High)
* **فایل‌های متاثر:** 
  - `src/Exceler/Pipeline/Write/Handlers/FormattingWriterHandler.cs` (خط ۱۷)
* **شرح ایراد:**
  کد زیر بدون شرط و بدون امکان غیرفعال‌سازی برای تمام ستون‌ها اجرا می‌شود:
  ```csharp
  context.Worksheet.Cells[context.Worksheet.Dimension.Address].AutoFitColumns();
  ```
* **پیامد:**
  محاسبه پهنای ستون بر اساس طول متون در اکسل نیازمند رندر فونت توسط GDI/موتور گرافیکی است. روی دیتاهای حجیم (مثلاً ۵۰,۰۰۰ سطر)، این فرایند زمان اکسپورت را از چند ثانیه به چند دقیقه افزایش داده و CPU را اشباع می‌کند. همچنین اگر شیت خالی باشد، `Dimension` مقدار `null` خواهد داشت و باعث پرتاب `NullReferenceException` می‌شود.

---

### ۳.۳. فیلتر کردن مداوم LINQ و خواندن مضاعف سلول‌ها در هر سطر
* **سطح اهمیت:** 🟡 متوسط (Medium)
* **فایل‌های متاثر:** 
  - `src/Exceler/Pipeline/Read/Handlers/ParseHandler.cs` (خط ۱۲)
  - `src/Exceler/Core/DefaultReader.cs` (خطوط ۱۳۳-۱۴۷)
* **شرح ایراد:**
  ```csharp
  foreach (var setter in context.Profile.CompiledSetters.Where(s => s.Key <= context.ColCount))
  ```
  فراخوانی `.Where(...)` در هر دور حلقه سطرها تکرار می‌شود (برای ۱۰۰ هزار سطر، ۱۰۰ هزار بار تولید اشیای موقت LINQ). علاوه بر آن، متد `IsRowEmpty` ابتدا سلول‌ها را خوانده و `.ToString()` می‌گیرد و سپس `ParseHandler` بلافاصله همان آدرس سلول‌ها را مجدداً از شیت می‌خواند.

---

## ۴. ایرادات قابلیت اطمینان، انواع داده و تبدیل‌ها (Reliability & Data Types)

### ۴.۱. تبدیل خاموش مقادیر خالی به صفر در انواع داده Primitive
* **سطح اهمیت:** 🔴 بحرانی (Data Corruption Risk)
* **فایل‌های متاثر:** 
  - `src/Exceler/Core/Converter/SafeConverter.cs` (خطوط ۷-۱۰)
* **شرح ایراد:**
  ```csharp
  public static T? ChangeType<T>(object? value)
  {
      if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
          return default;
  ```
* **پیامد:**
  اگر مدلی دارای پراپرتی `public int Age { get; set; }` (غیر نال‌پذیر) باشد و سلول اکسل خالی باشد، `default` یعنی عدد `0` بازگردانده می‌شود! سیستم هیچ خطایی مبنی بر خالی بودن فیلد اجباری ثبت نمی‌کند و دیتا با مقدار نادرست صفر ثبت می‌شود.

---

### ۴.۲. وابستگی خطرناک تبدیل تاریخ و اعشار به Culture سرور
* **سطح اهمیت:** 🟠 بالا (High)
* **فایل‌های متاثر:** 
  - `src/Exceler/Core/Converter/SafeConverter.cs` (خط ۲۲ و ۴۵)
* **شرح ایراد:**
  ```csharp
  DateTime.TryParse(value.ToString(), out DateTime parsedDate)
  Convert.ChangeType(value, targetType)
  ```
  هیچ مشخصه‌ای از `CultureInfo.InvariantCulture` ارسال نشده است.
* **پیامد:**
  تاریخی مثل `"01/02/2026"` در سرور با ریجن آمریکا (en-US) برابر با **دوم ژانویه** و در سرور بریتانیا یا استرالیا (en-GB) برابر با **اول فوریه** ذخیره خواهد شد! اعداد اعشاری نیز با ممیز یا کاما بسته به سرور دچار اختلال می‌شوند.

---

### ۴.۳. ریسک کرش در کانتینر داکر / سرورهای لینوکس به دلیل `System.Drawing.Color`
* **سطح اهمیت:** 🟠 بالا (High)
* **فایل‌های متاثر:** 
  - `src/Exceler/Configuration/ColumnBuilder.cs` (خط ۴، ۷۶، ۸۲)
  - `src/Exceler/Pipeline/Write/Handlers/StyleWriterHandler.cs`
* **شرح ایراد:**
  استفاده مستقیم از `System.Drawing.Color`. در محیط‌های مدرن دات‌نت (.NET 7, 8, 9)، مایکروسافت وابستگی `System.Drawing.Common` را در سیستم‌عامل‌های غیر ویندوزی مسدود کرده است.
* **پیامد:**
  در صورت دیپلوی پروژه در لینوکس یا کانتینر Docker، فراخوانی استایل‌دهی رنگ‌ها با خطای زیر متوقف می‌شود:
  `System.PlatformNotSupportedException: System.Drawing.Common is not supported on this platform.`
* **راهکار پیشنهادی:**
  استفاده از مقادیر Hex String (مثلاً `"#FF0000"`) یا ساختار رنگی مستقل.

---

### ۴.۴. عدم پشتیبانی از انواع مدرن دات‌نت (`DateOnly`, `TimeOnly`)
* **سطح اهمیت:** 🟡 متوسط (Medium)
* **فایل‌های متاثر:** 
  - `src/Exceler/Core/Converter/SafeConverter.cs`
* **شرح ایراد:**
  هیچ منطقی برای تبدیل به `DateOnly` و `TimeOnly` تعریف نشده است. با توجه به اینکه پروژه تارگت `.NET 6+` دارد، استفاده از این انواع استاندارد با خطای `InvalidCastException` مواجه خواهد شد.

---

### ۴.۵. پنهان ماندن استثناها در `ParseHandler`
* **سطح اهمیت:** 🟡 متوسط (Medium)
* **فایل‌های متاثر:** 
  - `src/Exceler/Pipeline/Read/Handlers/ParseHandler.cs` (خط ۲۳)
* **شرح ایراد:**
  هندلر فقط خطای `ExcelCastException` را Catch می‌کند:
  ```csharp
  catch (ExcelCastException)
  ```
  اگر یک کانورتر سفارشی (`IExcelValueConverter`) یا Property Setter مدل، استثنای دیگری مانند `FormatException`, `ArgumentException` یا `NullReferenceException` پرتاب کند، کل فرآیند خواندن فایل با کرش متوقف می‌شود و هیچ پیامی در خطاهای ردیف ثبت نمی‌گردد.

---

## ۵. ایرادات طراحی API و تجربه توسعه‌دهنده (API Design & DX)

### ۵.۱. هاردکد شدن جهت شیت به چپ‌به‌راست (`RightToLeft = false`)
* **سطح اهمیت:** 🟡 متوسط (Medium / Local Experience)
* **فایل‌های متاثر:** 
  - `src/Exceler/Pipeline/Write/Handlers/FormattingWriterHandler.cs` (خط ۱۶)
* **شرح ایراد:**
  ```csharp
  context.Worksheet.View.RightToLeft = false;
  ```
  هیچ تنظیمی در پروفایل برای فعال کردن راست‌به‌چپ (RTL) وجود ندارد. تمام خروجی‌های فارسی یا عربی به صورت چپ‌به‌راست تولید می‌شوند و کاربر راهی برای تغییر آن از طریق فریم‌ورک ندارد.

---

### ۵.۲. اعمال اشتباه استایل داده‌ها روی ردیف اول (هدر)
* **سطح اهمیت:** 🟡 متوسط (Medium)
* **فایل‌های متاثر:** 
  - `src/Exceler/Pipeline/Write/Handlers/StyleWriterHandler.cs` (خط ۲۱)
* **شرح ایراد:**
  ```csharp
  var range = context.Worksheet.Cells[1, colIndex, context.TotalRows, colIndex];
  ```
  بازه استایل‌دهی از ردیف ۱ آغاز می‌شود. این کار باعث می‌شود فرمت‌بندی اعداد (مانند Currency یا Date Format) و رنگ پس‌زمینه تعریف‌شده برای مقادیر، مستقیماً روی **متن سرستون (Header)** نیز اعمال و باعث تداخل ظاهری شود. ردیف شروع باید از ۲ باشد.

---

### ۵.۳. پیام‌های خطای مبهم با غلط املایی در هدر
* **سطح اهمیت:** 🟢 جزئی (Low / Polish)
* **فایل‌های متاثر:** 
  - `src/Exceler/Core/DefaultReader.cs` (خط ۱۷۵)
* **شرح ایراد:**
  ```csharp
  errors.Add("Input Header is not equal with excepted Header");
  ```
  - واژه `excepted` غلط املایی است و باید `expected` باشد.
  - پیام مشخص نمی‌کند کدام ستون در چه شماره اندیسی با چه مقداری مغایرت دارد. اگر چند ستون نامعتبر باشند، چند پیام تکراری و مبهم به کاربر نشان داده می‌شود.

---

### ۵.۴. فقدان متدهای پرکاربرد بدون فرآیند اضافه (Convenience APIs)
* **سطح اهمیت:** 🟡 متوسط (Medium)
* **فایل‌های متاثر:** 
  - `src/Exceler/Abstractions/IExcelReader.cs`
  - `src/Exceler/Abstractions/IExcelWriter.cs`
* **شرح ایراد:**
  عدم وجود متد ساده `Read<TModel>(stream)` یا `ReadAsync<TModel>(stream)`.
  همچنین متد `Write` در رایتر مقدار `Task<byte[]>` برمی‌گرداند در حالی که طبق استانداردهای دات‌نت (TAP) باید پسوند `Async` داشته باشد (`WriteAsync`).

---

## ۶. ایرادات و خلأهای پروژه تست (Testing Gaps)

### ۶.۱. تارگت قدیمی پروژه تست (`net6.0`)
* **فایل:** `tests/Exceler.Tests.Unit/Exceler.Tests.csproj`
* **شرح:** فریم‌ورک تست روی `net6.0` تنظیم شده است که منقضی (EOL) شده است؛ در حالی که کتابخانه اصلی تارگت‌های `net7.0`, `net8.0`, `net9.0` دارد. هیچ‌کدام از قابلیت‌ها و تغییرات رفتاری دات‌نت‌های ۸ و ۹ در فرایند تست بررسی نمی‌شوند.

### ۶.۲. عدم تست همزمانی (Concurrency) برای متد `EnsureBuilt`
* **شرح:** پروفایل‌ها Singleton هستند و اولیه‌سازی آن‌ها با `lock(this)` انجام می‌شود. هیچ تستی برای فراخوانی همزمان توسط چندین ترد جهت جلوگیری از Race Condition و Deadlock وجود ندارد.

### ۶.۳. نبود تست عدم ایجاد NullReferenceException در شیت‌های فاقد سلول
* **شرح:** اگر شیت کاملاً خالی بدون هدر ارسال شود، خصوصیت `Dimension` نال خواهد بود و در خط `Dimension.Address` استثنا رخ می‌دهد که این سناریو تست نشده است.

---

## ۷. جدول ماتریس اولویت‌بندی رفع اشکالات

| ردیف | شرح ایراد | لایه / کامپوننت | شدت | اولویت اقدام |
| :--- | :--- | :--- | :---: | :---: |
| ۱ | بازنویسی هاردکد لایسنس تجاری به NonCommercial | Licensing / Core | 🔴 Critical | فوری |
| ۲ | تبدیل خاموش مقادیر خالی به صفر در انواع غیر Nullable | Data Converter | 🔴 Critical | فوری |
| ۳ | عدم پشتیبانی از Async در IExcelValidator و IExcelProcessor | Abstractions | 🟠 High | بالا |
| ۴ | لود کامل فایل در رم و ادعای اشتباه Streaming | Architecture | 🟠 High | بالا |
| ۵ | ریسک کرش در لینوکس به دلیل System.Drawing.Color | Configuration | 🟠 High | بالا |
| ۶ | خطای Culture در تبدیل تاریخ‌ها و اعشار | Data Converter | 🟠 High | بالا |
| ۷ | عدم امکان شخصی‌سازی Pipeline (نقض OCP) | Pipeline | 🟠 High | بالا |
| ۸ | فریز ناشی از AutoFitColumns در دیتاهای بزرگ | Pipeline Writer | 🟠 High | بالا |
| ۹ | عدم پشتیبانی از چندین پروفایل برای یک مدل | Dependency Injection | 🟠 High | بالا |
| ۱۰ | وابستگی شدید به Service Locator در Reader و Writer | Core | 🟠 High | بالا |
| ۱۱ | اعمال استایل داده‌ها روی ردیف هدر | Pipeline Writer | 🟡 Medium | میان‌مدت |
| ۱۲ | هاردکد بودن RightToLeft = false | Pipeline Writer | 🟡 Medium | میان‌مدت |
| ۱۳ | عدم پشتیبانی از DateOnly و TimeOnly | Data Converter | 🟡 Medium | میان‌مدت |
| ۱۴ | محدودیت غیرضروری `new()` در متدهای Writer | Abstractions | 🟡 Medium | میان‌مدت |
| ۱۵ | عدم وجود متدهای خواندن ساده بدون Processor | Abstractions | 🟡 Medium | میان‌مدت |
| ۱۶ | اصلاح پیام‌های خطای هدر و غلط املایی | Core Reader | 🟢 Low | بهبود کد |
| ۱۷ | به‌روزرسانی فریم‌ورک پروژه تست به .NET 8/9 | Unit Tests | 🟢 Low | بهبود کد |

---
*گزارش تهیه شده توسط مهندس نرم‌افزار ارشد برای بازبینی معماری و تضمین کیفیت پکیج Exceler.*
