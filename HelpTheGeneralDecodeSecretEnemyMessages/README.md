# Help the general decode secret enemy messages

General Patron is faced with a problem, his intelligence has intercepted some secret messages from the enemy but they
are all encrypted. Those messages are crucial to getting the jump on the enemy and winning the war. Luckily intelligence
also captured an encoding device as well. However even the smartest programmers weren't able to crack it though. So the
general is asking you, his most odd but brilliant programmer.

You can call the encoder like this.

```csharp
Console.WriteLine(Encoder.Encode("Hello World!"));
```

Our crypto-analysts kept poking at it and found some interesting patterns.

```csharp
Console.WriteLine(Encoder.Encode ("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"));
Console.WriteLine(Encoder.Encode ("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"));
Console.WriteLine(Encoder.Encode ("!@#$%^&*()_+-"));

StringBuilder a = new StringBuilder();
StringBuilder b = new StringBuilder();
StringBuilder c = new StringBuilder();
foreach (char w in "abcdefghijklmnopqrstuvwxyz") {
    a.Append(Encoder.Encode (  "" + w)[0]);
    b.Append(Encoder.Encode ( "_" + w)[1]);
    c.Append(Encoder.Encode ("__" + w)[2]);
}

Console.WriteLine(a);
Console.WriteLine(b);
Console.WriteLine(c);
```

We think if you keep on this trail you should be able to crack the code! You are expected to fill in the function.

```csharp
public static string Decode(string p_what)
```

Good luck ! General Patron is counting on you!
