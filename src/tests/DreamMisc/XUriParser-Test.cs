/*
 * MindTouch Dream - a distributed REST framework 
 * Copyright (C) 2006-2013 MindTouch, Inc.
 * www.mindtouch.com  oss@mindtouch.com
 *
 * For community documentation and downloads visit wiki.developer.mindtouch.com;
 * please review the licensing section.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using MindTouch.Web;
using NUnit.Framework;

namespace MindTouch.Dream.Test {

    [TestFixture]
    public class XUriParserTest {

        // MISSING TESTS FOR:
        // * encoded user & password values
        // * test trailing double-slash: //

        //--- Class Methods ---
        private static XUri TryParse(string text) {
            string scheme;
            string user;
            string password;
            string hostname;
            int port;
            bool usesDefaultPort;
            bool trailingSlash;
            string[] segements;
            string query;
            string fragment;
            if(!XUriParser.TryParse(text, out scheme, out user, out password, out hostname, out port, out usesDefaultPort, out segements, out trailingSlash, out query, out fragment)) {
                Assert.Fail("failed to parse uri: {0}", text);
            }
            if(user != null) {
                user = XUri.Decode(user);
            }
            if(password != null) {
                password = XUri.Decode(password);
            }
            var result = new XUri(scheme, user, password, hostname, port, segements, trailingSlash, null, fragment);
            if(query != null) {
                result = result.WithQuery(query);
            }
            return result;
        }

        private void AssertParse(string text, string scheme = null, string user = null, string password = null, string hostname = null, int? port = null, bool? usesDefaultPort = null, string[] segments = null, bool? trailingSlash = null, KeyValuePair<string, string>[] @params = null, string fragment = null) {

            // setup
            Action<XUri, string> assert = (uri, suffix) => {
                Assert.AreEqual(scheme, uri.Scheme, string.Format("scheme ({0})", suffix));
                Assert.AreEqual(hostname, uri.Host, string.Format("hostname ({0})", suffix));
                Assert.AreEqual(port, uri.Port, string.Format("port ({0})", suffix));
                Assert.AreEqual(usesDefaultPort, uri.UsesDefaultPort, string.Format("usesDefaultPort ({0})", suffix));
                Assert.AreEqual(user, uri.User, string.Format("user ({0})", suffix));
                Assert.AreEqual(password, uri.Password, string.Format("password ({0})", suffix));
                Assert.AreEqual(segments, uri.Segments, string.Format("segments ({0})", suffix));
                Assert.AreEqual(trailingSlash, uri.TrailingSlash, string.Format("trailingSlash ({0})", suffix));
                Assert.AreEqual(@params, uri.Params, string.Format("query ({0})", suffix));
                Assert.AreEqual(fragment, uri.Fragment, string.Format("fragment ({0})", suffix));
                Assert.AreEqual(text, uri.ToString(), string.Format("ToString() ({0})", suffix));
            };

            // setup
            var uriOriginal = XUri.TryParse(text);
            var uriNew = TryParse(text);

            // test
            assert(uriOriginal, "original");
            assert(uriNew, "new");
        }

        //--- Methods ---

        [Test]
        public void TestUriConstructor1() {
            const string original = "http://domain.org/";
            AssertParse(original, 
                scheme: "http",
                hostname: "domain.org",
                port: 80,
                usesDefaultPort: true,
                segments: new string[0], 
                trailingSlash: true
            );
        }

        [Test]
        public void TestUriConstructor2() {
            const string original = "http://domain.org:81";
            AssertParse(original,
                scheme: "http",
                hostname: "domain.org",
                port: 81,
                usesDefaultPort: false,
                segments: new string[0],
                trailingSlash: false
            );
        }

        [Test]
        public void TestUriConstructor3() {
            const string original = "http://[2001:0db8:85a3:08d3:1319:8a2e:0370:7344]/";
            AssertParse(original,
                scheme: "http",
                hostname: "[2001:0db8:85a3:08d3:1319:8a2e:0370:7344]",
                port: 80,
                usesDefaultPort: true,
                segments: new string[0],
                trailingSlash: true
            );
        }

        [Test]
        public void TestUriConstructor4() {
            const string original = "http://[2001:0db8:85a3:08d3:1319:8a2e:0370:7344]:81/";
            AssertParse(original,
                scheme: "http",
                hostname: "[2001:0db8:85a3:08d3:1319:8a2e:0370:7344]",
                port: 81,
                usesDefaultPort: false,
                segments: new string[0],
                trailingSlash: true
            );
        }

        [Test]
        public void TestUriConstructor5() {
            const string original = "http://user:password@domain.org";
            AssertParse(original,
                scheme: "http",
                user: "user",
                password: "password",
                hostname: "domain.org",
                port: 80,
                usesDefaultPort: true,
                segments: new string[0],
                trailingSlash: false
            );
        }

        [Test]
        public void TestUriConstructor6() {
            const string original = "http://user:password@domain.org:81/";
            AssertParse(original,
                scheme: "http",
                user: "user",
                password: "password",
                hostname: "domain.org",
                port: 81,
                usesDefaultPort: false,
                segments: new string[0],
                trailingSlash: true
            );
        }

        [Test]
        public void TestUriConstructor7() {
            const string original = "http://user:password@domain.org:81/path";
            AssertParse(original,
                scheme: "http",
                user: "user",
                password: "password",
                hostname: "domain.org",
                port: 81,
                usesDefaultPort: false,
                segments: new[] { "path" },
                trailingSlash: false
            );
        }

        [Test]
        public void TestUriConstructor8() {
            const string original = "http://user:password@domain.org:81/path//";
            AssertParse(original,
                scheme: "http",
                user: "user",
                password: "password",
                hostname: "domain.org",
                port: 81,
                usesDefaultPort: false,
                segments: new[] { "path", "/" },
                trailingSlash: false
            );
        }

        [Test]
        public void TestUriConstructor9() {
            const string original = "http://user:password@domain.org:81/path/foo%20bar/path//@blah";
            AssertParse(original,
                scheme: "http",
                user: "user",
                password: "password",
                hostname: "domain.org",
                port: 81,
                usesDefaultPort: false,
                segments: new[] { "path", "foo%20bar", "path", "/@blah" },
                trailingSlash: false
            );
        }

        [Test]
        public void TestUriConstructor10() {
            const string original = "http://user:password@domain.org:81/path/foo%20bar/path//@blah?ready&set=&go=foo/bar";
            AssertParse(original,
                scheme: "http",
                user: "user",
                password: "password",
                hostname: "domain.org",
                port: 81,
                usesDefaultPort: false,
                segments: new[] { "path", "foo%20bar", "path", "/@blah" },
                trailingSlash: false,
                @params: new[] {
                    new KeyValuePair<string, string>("ready", null), 
                    new KeyValuePair<string, string>("set", ""), 
                    new KeyValuePair<string, string>("go", "foo/bar")
                }
            );
        }

        [Test]
        public void TestUriConstructor11() {
            const string original = "http://user:password@domain.org:81/path/foo%20bar/path//@blah#yo";
            AssertParse(original,
                scheme: "http",
                user: "user",
                password: "password",
                hostname: "domain.org",
                port: 81,
                usesDefaultPort: false,
                segments: new[] { "path", "foo%20bar", "path", "/@blah" },
                trailingSlash: false,
                fragment: "yo"
            );
        }

        [Test]
        public void TestUriConstructor12() {
            const string original = "http://user:password@domain.org:81/path/foo%20bar/path//@blah/?ready&set=&go=foo/bar#yo";
            AssertParse(original,
                scheme: "http",
                user: "user",
                password: "password",
                hostname: "domain.org",
                port: 81,
                usesDefaultPort: false,
                segments: new[] { "path", "foo%20bar", "path", "/@blah" },
                trailingSlash: true,
                @params: new[] {
                    new KeyValuePair<string, string>("ready", null), 
                    new KeyValuePair<string, string>("set", ""), 
                    new KeyValuePair<string, string>("go", "foo/bar")
                },
                fragment: "yo"
            );
        }

        [Test]
        public void TestUriConstructor13() {
            const string original = "ftp://ftp.is.co.za/rfc/rfc1808.txt";
            AssertParse(original,
                scheme: "ftp",
                hostname: "ftp.is.co.za",
                port: 21,
                usesDefaultPort: true,
                segments: new[] { "rfc", "rfc1808.txt" },
                trailingSlash: false
            );
        }

        [Test]
        public void TestUriConstructor14() {
            const string original = "http://www.ietf.org/rfc/rfc2396.txt";
            AssertParse(original,
                scheme: "http",
                hostname: "www.ietf.org",
                port: 80,
                usesDefaultPort: true,
                segments: new[] { "rfc", "rfc2396.txt" },
                trailingSlash: false
            );
        }

        [Test, Ignore("doesn't pass the original test either")]
        public void TestUriConstructor15() {
            const string original = "ldap://[2001:db8::7]/c=GB?objectClass?one";
            AssertParse(original,
                scheme: "ldap",
                hostname: "[2001:db8::7]",
                port: -1,
                usesDefaultPort: true,
                segments: new[] { "c=GB" },
                trailingSlash: false,
                @params: new[] {
                    new KeyValuePair<string, string>("objectClass?one", null)
                }
            );
        }

        [Test]
        public void TestUriConstructor16() {
            const string original = "telnet://192.0.2.16:80/";
            AssertParse(original,
                scheme: "telnet",
                hostname: "192.0.2.16",
                port: 80,
                usesDefaultPort: false,
                segments: new string[0],
                trailingSlash: true
            );
        }

        [Test]
        public void TestUriConstructor17() {
            const string original = "ftp://cnn.example.com&story=breaking_news@10.0.0.1/top_story.htm#";
            AssertParse(original,
                scheme: "ftp",
                user: "cnn.example.com&story=breaking_news",
                hostname: "10.0.0.1",
                port: 21,
                usesDefaultPort: true,
                segments: new[] { "top_story.htm" },
                trailingSlash: false,
                fragment: ""
            );
        }

        [Test]
        public void TestUriConstructor18() {
            const string original = "http://domain.org/?";
            AssertParse(original,
                scheme: "http",
                hostname: "domain.org",
                port: 80,
                usesDefaultPort: true,
                segments: new string[0],
                trailingSlash: true,
                @params: new KeyValuePair<string, string>[0]
            );
        }

        [Test]
        public void TestUriConstructor19() {
            const string original = "http://domain.org?";
            AssertParse(original,
                scheme: "http",
                hostname: "domain.org",
                port: 80,
                usesDefaultPort: true,
                segments: new string[0],
                trailingSlash: false,
                @params: new KeyValuePair<string, string>[0]
            );
        }

        [Test]
        public void TestUriConstructor20() {
            const string original = "http://www.ietf.org/rfc;15/rfc2396.txt";
            AssertParse(original,
                scheme: "http",
                hostname: "www.ietf.org",
                port: 80,
                usesDefaultPort: true,
                segments: new[] { "rfc;15", "rfc2396.txt" },
                trailingSlash: false
            );
        }

        [Test]
        public void TestUriConstructor21() {
            const string original = "http://www.ietf.org/rfc;15/rfc2396.txt;";
            AssertParse(original,
                scheme: "http",
                hostname: "www.ietf.org",
                port: 80,
                usesDefaultPort: true,
                segments: new[] { "rfc;15", "rfc2396.txt;" },
                trailingSlash: false
            );
        }

        [Test]
        public void TestUriConstructor22() {
            const string original = "http://www.ietf.org/;15/rfc2396.txt;";
            AssertParse(original,
                scheme: "http",
                hostname: "www.ietf.org",
                port: 80,
                usesDefaultPort: true,
                segments: new[] { ";15", "rfc2396.txt;" },
                trailingSlash: false
            );
        }

        [Test]
        public void TestUriConstructor23() {
            const string original = "http:///path";
            AssertParse(original,
                scheme: "http",
                hostname: "",
                port: 80,
                usesDefaultPort: true,
                segments: new[] { "path" },
                trailingSlash: false
            );
        }

        [Test]
        public void TestUriConstructor24() {
            const string original = "http://host/seg^ment?qu^ery=a|b^c#fo|o#b^ar";
            AssertParse(original,
                scheme: "http",
                hostname: "host",
                port: 80,
                usesDefaultPort: true,
                segments: new[] { "seg^ment" },
                trailingSlash: false,
                @params: new[] { new KeyValuePair<string, string>("qu^ery", "a|b^c") },
                fragment: "fo|o#b^ar"
            );
        }

        [Test]
        public void Can_parse_square_brackets_in_query() {
            Assert.IsNotNull(XUri.TryParse("http://host/foo?bar[123]=abc"));
        }

        [Test]
        public void Can_parse_square_brackets_in_segment() {
            Assert.IsNotNull(XUri.TryParse("http://host/foo/[123]/bar"));
        }

        [Test]
        public void Can_parse_square_brackets_in_fragment() {
            Assert.IsNotNull(XUri.TryParse("http://host/foo#[bar]"));
        }

        [Test]
        public void Square_brackets_in_parsed_query_are_encoded_on_render() {
            Assert.AreEqual("http://host/foo?bar%5B123%5D=abc",new XUri("http://host/foo?bar[123]=abc").ToString());
        }

        [Test]
        public void Can_parse_curly_brackets_in_query() {
            Assert.IsNotNull(XUri.TryParse("http://test.com/AllItems.aspx?RootFolder={xyz}"));
        }

        [Test]
        public void Can_parse_curly_brackets_in_segment() {
            Assert.IsNotNull(XUri.TryParse("http://test.com/{xyz}/foo"));
        }

        [Test]
        public void Can_parse_curly_brackets_in_fragment() {
            Assert.IsNotNull(XUri.TryParse("http://test.com/foo#{xyz}"));
        }

        [Test]
        public void Curly_brackets_in_parsed_query_are_encoded_on_render() {
            Assert.AreEqual("http://test.com/AllItems.aspx?RootFolder=%7Bxyz%7D", new XUri("http://test.com/AllItems.aspx?RootFolder={xyz}").ToString());
        }

        [Test]
        public void TestXUriFromUriConstruction() {
            var evilSegments = new[] {

                // Escaped version of "I�t�rn�ti�n�liz�ti�n" (should look similar to "Internationalization" but with extended characteres)
                "I\u00f1t\u00ebrn\u00e2ti\u00f4n\u00e0liz\u00e6ti\u00f8n",
                "A%4b",
                "A^B"
            };
            foreach(var evil in evilSegments) {
                var original = new Uri("http://foo/" + evil);
                var fromDecoded = new Uri(original.ToString());
                var uri1 = new XUri(original);
                var uri2 = new XUri(fromDecoded);
                // just making sure they actually parse
            }
        }

        [Test]
        public void TestXUriFromUriConstruction2() {
            var evilSegments = new[] {

                // Escaped version of "I�t�rn�ti�n�liz�ti�n" (should look similar to "Internationalization" but with extended characteres)
                "I\u00f1t\u00ebrn\u00e2ti\u00f4n\u00e0liz\u00e6ti\u00f8n",
                "A%4b"
            };
            foreach(var evil in evilSegments) {
                var original = new XUri("http://" + evil);
                var fromDecoded = new XUri(original.ToString());
                var uri1 = new XUri(original);
                var uri2 = new XUri(fromDecoded);
                // just making sure they actually parse
            }
        }

        [Test]
        public void Decode_extended_chars1() {
            const string before = "F\u00F4\u00F6";
            const string after = "F\u00F4\u00F6";
            var decoded = XUri.Decode(before);
            Assert.AreEqual(after, decoded);
        }

        [Test]
        public void Decode_extended_chars2() {
            const string before = "F%20\u00F4%20\u00F6";
            const string after = "F \u00F4 \u00F6";
            var decoded = XUri.Decode(before);
            Assert.AreEqual(after, decoded);
        }

        [Test]
        public void Decode_extended_chars3() {
            const string before = "+F\u00F4\u00F6";
            const string after = " F\u00F4\u00F6";
            var decoded = XUri.Decode(before);
            Assert.AreEqual(after, decoded);
        }

        [Test]
        public void Decode_extended_chars4() {
            const string before = "f%7Bb\u00FCb";
            const string after = "f{b\u00FCb";
            var decoded = XUri.Decode(before);
            Assert.AreEqual(after, decoded);
        }

        [Test]
        public void EncodeSegment() {
            Assert.AreEqual("a^b", XUri.EncodeSegment("a^b"));
        }

        [Test]
        public void EncodeQuery() {
            Assert.AreEqual("a^b|c", XUri.EncodeQuery("a^b|c"));
        }

        [Test]
        public void EncodeFragment() {
            Assert.AreEqual("a^b|c#d", XUri.EncodeFragment("a^b|c#d"));
        }

        [Test]
        public void TestAppendPath1() {
            var uri = new XUri("http://www.dummy.com:8081/first/second");
            uri = uri.AtPath("foo/bar");
            Assert.AreEqual("http://www.dummy.com:8081/first/second/foo/bar", uri.ToString());
        }

        [Test]
        public void TestAppendPath2() {
            var uri = new XUri("http://www.dummy.com:8081/first/second");
            uri = uri.AtPath("/foo/bar");
            Assert.AreEqual("http://www.dummy.com:8081/first/second//foo/bar", uri.ToString());
        }

        [Test]
        public void TestAppendPath3() {
            var uri = new XUri("http://www.dummy.com:8081/first/second?query=arg");
            uri = uri.AtPath("foo/bar");
            Assert.AreEqual("http://www.dummy.com:8081/first/second/foo/bar?query=arg", uri.ToString());
        }

        [Test]
        public void TestAppendPath4() {
            var uri = new XUri("http://www.dummy.com:8081/first/second?query=arg");
            uri = uri.AtPath("foo/bar?q=a");
            Assert.AreEqual("http://www.dummy.com:8081/first/second/foo/bar?query=arg&q=a", uri.ToString());
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAppendPath5() {
            var uri = new XUri("http://www.dummy.com:8081/first/second?query=arg");
            uri = uri.At("foo/bar");
        }

        [Test]
        public void TestAppendPath6() {
            var uri = new XUri("http:///").At("path");
            Assert.AreEqual("/path", uri.Path);
            Assert.AreEqual(1, uri.Segments.Length);
            Assert.AreEqual("http:///path", uri.ToString());
        }

        [Test]
        public void TestAppendPath7() {
            var uri = new XUri("http:///").AtAbsolutePath("foo/bar");
            Assert.AreEqual("/foo/bar", uri.Path);
            Assert.AreEqual(2, uri.Segments.Length);
            Assert.AreEqual("http:///foo/bar", uri.ToString());
        }

        [Test]
        public void TestAppendPath8() {
            var uri = new XUri("http:///").AtAbsolutePath("foo/bar/");
            Assert.AreEqual("/foo/bar/", uri.Path);
            Assert.AreEqual(2, uri.Segments.Length);
            Assert.AreEqual("http:///foo/bar/", uri.ToString());
        }

        [Test]
        public void TestSetPath1() {
            var uri = new XUri("http://www.dummy.com:8081/first/second?query=arg");
            uri = uri.AtAbsolutePath("foo/bar?q=a");
            Assert.AreEqual("http://www.dummy.com:8081/foo/bar?q=a", uri.ToString());
        }

        [Test]
        public void TestSetPath2() {
            var uri = new XUri("http://www.dummy.com:8081/first/second?query=arg");
            uri = uri.AtAbsolutePath("/foo/bar?q=a");
            Assert.AreEqual("http://www.dummy.com:8081/foo/bar?q=a", uri.ToString());
        }

        [Test]
        public void TestAppendQuery1() {
            var uri = new XUri("http://www.dummy.com:8081/first/second");
            uri = uri.With("query", "arg");
            Assert.AreEqual("http://www.dummy.com:8081/first/second?query=arg", uri.ToString());
        }

        [Test]
        public void TestAppendQuery2() {
            var uri = new XUri("http://www.dummy.com:8081/first/second?query=arg");
            uri = uri.With("q", "a");
            Assert.AreEqual("http://www.dummy.com:8081/first/second?query=arg&q=a", uri.ToString());
        }

        [Test]
        public void TestTryParse() {
            Assert.IsFalse(XUri.TryParse("htt;//") != null);
        }

        [Test]
        public void TestEquality() {
            Assert.AreEqual(new XUri("HTTP://LOCALHOST/FOO/BAR"), new XUri("http://localhost:80/foo/bar"), "==");
        }

        [Test]
        public void TestEquality1() {
            Assert.AreNotEqual(new XUri("HTTPS://LOCALHOST/FOO/BAR"), new XUri("http://localhost:80/foo/bar"), "!=");
        }

        [Test]
        public void TestEquality2() {
            var a = new XUri("http://foobar/?foo=bar");
            var b = new XUri("http://foobar?FOO=bar");
            Assert.AreEqual(a, b);
        }

        [Test]
        public void TestEquality3() {
            var a = new XUri("http://foobar/?foo=bar");
            var b = new XUri("http://foobar?foo=BAR");
            Assert.AreNotEqual(a, b);
        }

        [Test]
        public void TestEquality4() {
            var a = new XUri("http://foobar/?foo=a&bar=b");
            var b = new XUri("http://foobar?bar=b&foo=a");
            Assert.AreNotEqual(a, b);
        }

        [Test]
        public void TestEquality5() {
            var a = new XUri("http://user:password@foobar");
            var b = new XUri("http://USER:password@foobar");
            Assert.AreEqual(a, b);
        }

        [Test]
        public void TestEquality6() {
            var a = new XUri("http://user:password@foobar");
            var b = new XUri("http://user:PASSWORD@foobar");
            Assert.AreNotEqual(a, b);
        }

        [Test]
        public void TestEquality7() {
            var a = new XUri("http://foobar#fragment");
            var b = new XUri("http://foobar#FRAGMENT");
            Assert.AreEqual(a, b);
        }

        [Test]
        public void TestEquality8() {
            var a = new XUri("http://foobar/x#foo");
            var b = new XUri("http://foobar/x#bar");
            Assert.AreNotEqual(a, b);
        }

        [Test]
        public void TestHashcode() {
            Assert.AreEqual(new XUri("HTTP://LOCALHOST/FOO/BAR").GetHashCode(), new XUri("http://localhost:80/foo/bar").GetHashCode(), "==");
            Assert.AreNotEqual(new XUri("HTTPS://LOCALHOST/FOO/BAR").GetHashCode(), new XUri("http://localhost:80/foo/bar").GetHashCode(), "!=");
        }

        [Test]
        public void TestChangePrefix1() {
            var from = new XUri("http://from-uri/a/b/c");
            var to = new XUri("http://to-uri/x/y/z");
            Assert.AreEqual("http://user:pwd@to-uri/x/y/z/d/e?p=1#fragment", new XUri("http://user:pwd@from-uri/a/b/c/d/e?p=1#fragment").ChangePrefix(from, to).ToString(), "==");
        }

        [Test]
        public void TestChangePrefix2() {
            var from = new XUri("http://from-uri/a/b/c");
            var to = new XUri("http://to-uri/x/y/z");
            Assert.AreEqual("http://user:pwd@to-uri/x/y/z/../d/e?p=1#fragment", new XUri("http://user:pwd@from-uri/a/b/d/e?p=1#fragment").ChangePrefix(from, to).ToString(), "==");
        }

        [Test]
        public void TestToStringFalse() {
            var uri = new XUri("http://user:password@hostname/path");
            Assert.AreEqual("http://user:xxx@hostname/path", uri.ToString(false), "ToString(false)");
        }

        [Test]
        public void TestGethashcodeWithNullQueryParam() {
            var uri = new XUri("http://foobar").With("abc", null);
            var hashcode = uri.GetHashCode();
        }

        [Test]
        public void TestIpRecognition() {
            var uri = new XUri("http://192.168.1.12/foobar");
            Assert.IsTrue(uri.HostIsIp);
            uri = new XUri("http://123.123.123.com/abc");
            Assert.IsFalse(uri.HostIsIp);
        }

        [Test]
        public void TestFragmentEncoding() {
            var uri = new XUri("http://foo/bar#baz=10");
            Assert.AreEqual("baz=10", uri.Fragment);
            var uri2 = new XUri(uri.ToString());
            Assert.AreEqual("baz=10", uri2.Fragment);
        }

        [Test]
        public void TestQueryEncoding() {
            var uri = new XUri("http://foo/bar");
            uri = uri.With("x", "a=b");
            Assert.AreEqual("a=b", uri.GetParam("x"));
            var uri2 = new XUri(uri.ToString());
            Assert.AreEqual("a=b", uri2.GetParam("x"));
        }

        [Test]
        public void TestUriConversionForSegmentsEndingInDots() {
            var xuri = new XUri("http://server/foo.../bar");
            var uri = xuri.ToUri();

            Assert.AreEqual("http://server/foo%252E%252E%252E/bar", uri.ToString());
        }

        [Test]
        public void Uri_conversion_of_segments_containing_colon() {
            var xuri = new XUri("http://server/foo:baz/bar");
            var uri = xuri.ToUri();
            Assert.AreEqual("http://server/foo%253Abaz/bar", uri.ToString());
        }

        [Test]
        public void Uri_conversion_of_segments_containing_colon_with_double_encoding_turned_off() {
            var xuri = new XUri("http://server/foo:baz/bar").WithoutSegmentDoubleEncoding();
            var uri = xuri.ToUri();
            Assert.AreEqual("http://server/foo:baz/bar", uri.ToString());
        }

        [Test]
        public void Uri_conversion_of_segments_containing_colon_with_double_encoding_turned_off_and_back_on() {
            var xuri = new XUri("http://server/foo:baz/bar").WithoutSegmentDoubleEncoding().WithSegmentDoubleEncoding();
            var uri = xuri.ToUri();
            Assert.AreEqual("http://server/foo%253Abaz/bar", uri.ToString());
        }

        [Test]
        public void TestUriConversion() {
            var xuri = new XUri("http://user:password@server/foo/bar?query=param#fragment");
            var uri = xuri.ToUri();

            Assert.AreEqual("http://user:password@server/foo/bar?query=param#fragment", uri.ToString());
        }

        [Test]
        public void Windows_network_file_path_uri_with_backslashes() {
            var uri = new XUri(@"file:///\\deki-hayes\drive");
            Assert.AreEqual("file://///deki-hayes/drive", uri.ToString());
        }

        [Test]
        public void Windows_drive_file_path_uri_with_backslashes() {
            var uri = new XUri(@"file:///c:\temp\foo.txt");
            Assert.AreEqual(@"file:///c:/temp/foo.txt", uri.ToString());
        }

        [Test]
        public void Parsing_HttpContext_Uri_with_bad_chars() {
            const string rawPath = "/we have spaces and illegal chars: \"'`<>.jpg";
            var baduri = new Uri("http://foo.com/w/we have spaces and illegal chars: ^\"'`<>.jpg");
            var parsedUri = HttpUtil.FromHttpContextComponents(baduri, rawPath);
            Assert.AreEqual("http://foo.com/we%20have%20spaces%20and%20illegal%20chars:%20%22%27%60%3c%3e.jpg", parsedUri.ToString());
        }

        [Test]
        public void Can_append_trailing_slash() {
            var uri = new XUri("http://foo/bar").WithTrailingSlash();
            Assert.IsTrue(uri.TrailingSlash);
            Assert.AreEqual("http://foo/bar/", uri.ToString());
        }

        [Test]
        public void WithTrailingSlash_only_adds_when_needed() {
            var uri = new XUri("http://foo/bar/");
            Assert.IsTrue(uri.TrailingSlash);
            uri = uri.WithTrailingSlash();
            Assert.IsTrue(uri.TrailingSlash);
            Assert.AreEqual("http://foo/bar/", uri.ToString());
        }

        [Test]
        public void Can_remove_trailing_slash() {
            var uri = new XUri("http://foo/bar/").WithoutTrailingSlash();
            Assert.IsFalse(uri.TrailingSlash);
            Assert.AreEqual("http://foo/bar", uri.ToString());
        }

        [Test]
        public void WithoutTrailingSlash_only_removes_when_needed() {
            var uri = new XUri("http://foo/bar");
            Assert.IsFalse(uri.TrailingSlash);
            uri = uri.WithoutTrailingSlash();
            Assert.IsFalse(uri.TrailingSlash);
            Assert.AreEqual("http://foo/bar", uri.ToString());
        }

        [Test]
        public void GetRelativePathTo_respects_trailing_slash_on_source() {
            AssertRelative("http://foo/a/b/c/", "http://foo/a", "b/c/");
        }

        [Test]
        public void GetRelativePathTo_respects_trailing_slash_on_relative() {
            AssertRelative("http://foo/a/b/c", "http://foo/a/", "b/c");
        }

        [Test]
        public void GetRelativePathTo_respects_trailing_slash_on_source_on_source_and_target() {
            AssertRelative("http://foo/a/b/c/", "http://foo/a/", "b/c/");
        }

        [Test]
        public void GetRelativePathTo_respects_no_trailing_slash_on_source_or_target() {
            AssertRelative("http://foo/a/b/c", "http://foo/a", "b/c");
        }

        [Test]
        public void Reverse_GetRelativePathTo_respects_trailing_slash_on_source() {
            AssertRelative("http://foo/a", "http://foo/a/b/c/", "../..");
        }

        [Test]
        public void Reverse_GetRelativePathTo_respects_trailing_slash_on_relative() {
            AssertRelative("http://foo/a/", "http://foo/a/b/c", "../../");
        }

        [Test]
        public void Reverse_GetRelativePathTo_respects_trailing_slash_on_source_on_source_and_target() {
            AssertRelative("http://foo/a/", "http://foo/a/b/c/", "../../");
        }

        [Test]
        public void Reverse_GetRelativePathTo_respects_no_trailing_slash_on_source_or_target() {
            AssertRelative("http://foo/a", "http://foo/a/b/c", "../..");
        }

        [Test]
        public void Equal_uris_have_empty_commons_regarless_of_trailing_slashes() {
            AssertRelative("http://foo/a/b/c/", "http://foo/a/b/c/", "");
            AssertRelative("http://foo/a/b/c", "http://foo/a/b/c", "");
            AssertRelative("http://foo/a/b/c", "http://foo/a/b/c/", "");
            AssertRelative("http://foo/a/b/c/", "http://foo/a/b/c", "");
        }

        [Test]
        public void GetRelativePathTo_turns_double_slash_into_leading_slash() {
            AssertRelative("http://foo/a//b/c", "http://foo/a", "/b/c");
        }

        [Test]
        public void AtPath_keeps_leading_slash() {
            Assert.AreEqual("http://foo/a//b/c", new XUri("http://foo/a").AtPath("/b/c").ToString());
        }

        [Test]
        public void AtPath_respectes_trailing_slash() {
            Assert.AreEqual("http://foo/a/b/c/", new XUri("http://foo/a").AtPath("b/c/").ToString());
        }

        [Test]
        public void Can_roundtrip_GetRelativePathTo_via_AtPath() {
            var uri = new XUri("http://foo/a/b/c");
            var baseUri = new XUri("http://foo/a");
            var relative = uri.GetRelativePathTo(baseUri);
            var roundtrip = baseUri.AtPath(relative);
            Assert.AreEqual(uri.ToString(), roundtrip.ToString());
        }
        [Test]
        public void Can_roundtrip_GetRelativePathTo_via_AtPath_with_trailing_slash_on_uri() {
            var uri = new XUri("http://foo/a/b/c/");
            var baseUri = new XUri("http://foo/a");
            var relative = uri.GetRelativePathTo(baseUri);
            var roundtrip = baseUri.AtPath(relative);
            Assert.AreEqual(uri.ToString(), roundtrip.ToString());
        }

        [Test]
        public void Can_roundtrip_GetRelativePathTo_via_AtPath_with_trailing_slash_on_base() {
            var uri = new XUri("http://foo/a/b/c");
            var baseUri = new XUri("http://foo/a/");
            var relative = uri.GetRelativePathTo(baseUri);
            var roundtrip = baseUri.AtPath(relative);
            Assert.AreEqual(uri.ToString(), roundtrip.ToString());
        }

        [Test]
        public void Can_roundtrip_AtPath_via_GetRelativePathTo() {
            const string relative = "b/c";
            var baseUri = new XUri("http://foo/a");
            var combined = baseUri.AtPath(relative);
            Assert.AreEqual(relative, combined.GetRelativePathTo(baseUri));
        }

        [Test]
        public void Can_roundtrip_AtPath_via_GetRelativePathTo_with_trailing_slash_on_relative() {
            const string relative = "b/c/";
            var baseUri = new XUri("http://foo/a");
            var combined = baseUri.AtPath(relative);
            Assert.AreEqual(relative, combined.GetRelativePathTo(baseUri));
        }

        [Test]
        public void Can_roundtrip_AtPath_via_GetRelativePathTo_with_trailing_slash_on_base() {
            const string relative = "b/c";
            var baseUri = new XUri("http://foo/a/");
            var combined = baseUri.AtPath(relative);
            Assert.AreEqual(relative, combined.GetRelativePathTo(baseUri));
        }

        [Test]
        public void GetRelativePathTo_works_with_http_and_https() {
            var baseUri = new XUri("http://foo/a");
            var uri = new XUri("https://foo/a/b/c");
            var result = uri.GetRelativePathTo(baseUri);
            Assert.AreEqual("b/c", result);
        }

        [Test]
        public void HasPrefix_works_with_http_and_https() {
            var baseUri = new XUri("http://foo/a");
            var uri = new XUri("https://foo/a/b/c");
            var result = uri.HasPrefix(baseUri);
            Assert.IsTrue(result);
        }

        [Test]
        public void ChangePrefix_works_with_http_and_https() {
            var fromUri = new XUri("http://foo/from");
            var toUri = new XUri("http://foo/to");
            var uri = new XUri("https://foo/from/a/b/c");
            var result = uri.ChangePrefix(fromUri, toUri);
            Assert.AreEqual("http://foo/to/a/b/c", result.ToString());
        }


        [Test]
        public void Similarity_works_with_http_and_https() {
            var httpsUri = new XUri("https://foo/a/b/c");
            var httpUri = new XUri("http://foo/a/b/c");
            var uri = new XUri("https://foo/a/b/c/d");
            var resultHttps = uri.Similarity(httpsUri);
            var resultHttp = uri.Similarity(httpUri);
            Assert.AreEqual(resultHttp, resultHttps);
        }

        [Test]
        public void HasPrefix_does_not_work_with_http_and_https_using_strict() {
            var baseUri = new XUri("http://foo/a");
            var uri = new XUri("https://foo/a/b/c");
            var result = uri.HasPrefix(baseUri, true);
            Assert.IsFalse(result);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void ChangePrefix_does_not_work_with_http_and_https_using_strict() {
            var fromUri = new XUri("http://foo/from");
            var toUri = new XUri("http://foo/to");
            var uri = new XUri("https://foo/from/a/b/c");
            var result = uri.ChangePrefix(fromUri, toUri, true);
            Assert.AreNotEqual("http://foo/to/a/b/c", result.ToString());
        }

        [Test]
        public void Similarity_does_not_work_with_http_and_https_using_strict() {
            var httpsUri = new XUri("https://foo/a/b/c");
            var httpUri = new XUri("http://foo/a/b/c");
            var uri = new XUri("https://foo/a/b/c/d");
            var resultHttps = uri.Similarity(httpsUri, true);
            var resultHttp = uri.Similarity(httpUri, true);
            Assert.AreNotEqual(resultHttp, resultHttps);
        }

        private void AssertRelative(string uri, string relative, string common) {
            var x = new XUri(uri).GetRelativePathTo(new XUri(relative));
            Assert.AreEqual(common, x, string.Format("{0} relative to {1}", uri, relative));
        }
    }
}
